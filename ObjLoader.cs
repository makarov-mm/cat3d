using System.Globalization;
using System.Numerics;

namespace Cat3d;

// Reads the triangles of a Wavefront OBJ file and the texture maps of its material.
// Faces with more than three corners are split into fans, so they should be convex. The
// whole mesh gets the first material its faces use. Corners without a normal get the
// average of the face normals around their position.
internal static class ObjLoader
{
    public static ObjMesh Load(string path)
    {
        var positions = new List<Vector3>();
        var texCoords = new List<Vector2>();
        var normals = new List<Vector3>();

        // Every distinct position/texture/normal combination of the face corners is one vertex.
        var cornerVertices = new Dictionary<(int Position, int TexCoord, int Normal), uint>();
        var vertices = new List<ObjVertex>();
        var vertexPositions = new List<int>();
        var indices = new List<uint>();
        var face = new List<uint>();
        string? materialLibrary = null;
        string? material = null;
        int lineNumber = 0;

        foreach (string text in File.ReadLines(path))
        {
            lineNumber++;
            ReadOnlySpan<char> line = text;
            try
            {
                switch (NextWord(ref line))
                {
                    case "v":
                        positions.Add(new Vector3(ReadFloat(ref line), ReadFloat(ref line), ReadFloat(ref line)));
                        break;
                    case "vt":
                        // OBJ counts v from the bottom of the image up; the textures are given to
                        // OpenGL from the top row down.
                        texCoords.Add(new Vector2(ReadFloat(ref line), 1 - ReadFloat(ref line)));
                        break;
                    case "vn":
                        normals.Add(new Vector3(ReadFloat(ref line), ReadFloat(ref line), ReadFloat(ref line)));
                        break;
                    case "f":
                        face.Clear();
                        for (ReadOnlySpan<char> corner = NextWord(ref line); !corner.IsEmpty; corner = NextWord(ref line))
                            face.Add(CornerVertex(corner));
                        if (face.Count < 3)
                            throw new FormatException("A face needs three corners or more.");
                        for (int i = 2; i < face.Count; i++)
                        {
                            indices.Add(face[0]);
                            indices.Add(face[i - 1]);
                            indices.Add(face[i]);
                        }
                        break;
                    case "mtllib":
                        materialLibrary ??= line.Trim().ToString();
                        break;
                    case "usemtl":
                        material ??= line.Trim().ToString();
                        break;
                }
            }
            catch (Exception error) when (error is FormatException or OverflowException)
            {
                throw new InvalidDataException($"{Path.GetFileName(path)}, line {lineNumber}: {error.Message}", error);
            }
        }

        if (indices.Count == 0)
            throw new InvalidDataException($"{Path.GetFileName(path)} has no faces.");
        if (vertices.Exists(vertex => vertex.Normal == Vector3.Zero))
            AddMissingNormals(vertices, vertexPositions, positions, indices);

        string? diffuseMap = null, bumpMap = null;
        if (materialLibrary is not null)
        {
            string libraryPath = Path.Combine(Path.GetDirectoryName(path) ?? "", materialLibrary);
            if (File.Exists(libraryPath))   // without it the model is drawn untextured
                (diffuseMap, bumpMap) = ReadMaterial(libraryPath, material);
        }

        return new ObjMesh([.. vertices], [.. indices], diffuseMap, bumpMap);

        // The vertex of a face corner: p, p/t, p//n or p/t/n, with indices counted from 1, or
        // back from the latest when negative.
        uint CornerVertex(ReadOnlySpan<char> corner)
        {
            Span<Range> parts = stackalloc Range[4];
            int count = corner.Split(parts, '/');
            if (count > 3)
                throw new FormatException($"'{corner}' is not a face corner.");
            int position = ResolveIndex(corner[parts[0]], positions.Count);
            int texCoord = count > 1 && !corner[parts[1]].IsEmpty ? ResolveIndex(corner[parts[1]], texCoords.Count) : -1;
            int normal = count > 2 && !corner[parts[2]].IsEmpty ? ResolveIndex(corner[parts[2]], normals.Count) : -1;

            if (!cornerVertices.TryGetValue((position, texCoord, normal), out uint vertex))
            {
                vertex = (uint)vertices.Count;
                cornerVertices.Add((position, texCoord, normal), vertex);
                vertices.Add(new ObjVertex(
                    positions[position],
                    normal >= 0 ? normals[normal] : Vector3.Zero,   // filled in at the end
                    texCoord >= 0 ? texCoords[texCoord] : Vector2.Zero));
                vertexPositions.Add(position);
            }
            return vertex;
        }
    }

    // Gives the vertices without a normal the area-weighted average of the normals of the
    // faces around their position.
    private static void AddMissingNormals(List<ObjVertex> vertices, List<int> vertexPositions, List<Vector3> positions, List<uint> indices)
    {
        var sums = new Vector3[positions.Count];
        for (int i = 0; i < indices.Count; i += 3)
        {
            int a = vertexPositions[(int)indices[i]];
            int b = vertexPositions[(int)indices[i + 1]];
            int c = vertexPositions[(int)indices[i + 2]];
            Vector3 normal = Vector3.Cross(positions[b] - positions[a], positions[c] - positions[a]);   // twice the area long
            sums[a] += normal;
            sums[b] += normal;
            sums[c] += normal;
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            ObjVertex vertex = vertices[i];
            Vector3 sum = sums[vertexPositions[i]];
            if (vertex.Normal == Vector3.Zero && sum != Vector3.Zero)
                vertices[i] = new ObjVertex(vertex.Position, Vector3.Normalize(sum), vertex.TexCoord);
        }
    }

    // The diffuse and bump map files of a material in an MTL file, of its first material
    // when `material` is null. Map options are skipped: the file name is the last word.
    private static (string? Diffuse, string? Bump) ReadMaterial(string path, string? material)
    {
        string directory = Path.GetDirectoryName(path) ?? "";
        string? diffuse = null, bump = null;
        bool inMaterial = false;
        foreach (string text in File.ReadLines(path))
        {
            ReadOnlySpan<char> line = text;
            switch (NextWord(ref line))
            {
                case "newmtl":
                    if (inMaterial)
                        return (diffuse, bump);
                    inMaterial = material is null || line.Trim().SequenceEqual(material);
                    break;
                case "map_Kd" when inMaterial:
                    diffuse ??= MapFile(line, directory);
                    break;
                case "map_bump" or "bump" when inMaterial:
                    bump ??= MapFile(line, directory);
                    break;
            }
        }
        return (diffuse, bump);
    }

    private static string? MapFile(ReadOnlySpan<char> arguments, string directory)
    {
        arguments = arguments.TrimEnd();
        ReadOnlySpan<char> file = arguments[(arguments.LastIndexOfAny(' ', '\t') + 1)..];
        return file.IsEmpty ? null : Path.Combine(directory, file.ToString());
    }

    // The next word of `line`, which keeps the rest; empty at the end of the line.
    private static ReadOnlySpan<char> NextWord(ref ReadOnlySpan<char> line)
    {
        line = line.TrimStart();
        int end = line.IndexOfAny(' ', '\t');
        if (end < 0)
            end = line.Length;
        ReadOnlySpan<char> word = line[..end];
        line = line[end..];
        return word;
    }

    private static float ReadFloat(ref ReadOnlySpan<char> line)
    {
        ReadOnlySpan<char> word = NextWord(ref line);

        return word.IsEmpty
            ? throw new FormatException("A number is missing.")
            : float.Parse(word, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    // An OBJ index as a 0-based index into a list of `count` items.
    private static int ResolveIndex(ReadOnlySpan<char> text, int count)
    {
        int index = int.Parse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        int resolved = index > 0 ? index - 1 : count + index;

        return index != 0 && resolved >= 0 && resolved < count
            ? resolved
            : throw new FormatException($"Index {index} is out of range.");
    }
}
