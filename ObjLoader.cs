using System.Globalization;
using System.Numerics;

namespace Cat3d;

public static class ObjLoader
{
    public static ObjMesh Load(string objPath)
    {
        if (!File.Exists(objPath))
            throw new FileNotFoundException("OBJ file not found.", objPath);

        string objDir = Path.GetDirectoryName(objPath) ?? "";

        var positions = new List<Vector3>();
        var texCoords = new List<Vector2>();
        var normals = new List<Vector3>();
        var vertices = new List<ObjVertex>();

        string? mtlFileName = null;
        string? currentMaterial = null;

        foreach (string rawLine in File.ReadAllLines(objPath))
        {
            string line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#"))
                continue;

            string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            switch (parts[0])
            {
                case "mtllib":
                    if (parts.Length >= 2)
                        mtlFileName = parts[1];
                    break;

                case "usemtl":
                    if (parts.Length >= 2)
                        currentMaterial = parts[1];
                    break;

                case "v":
                    positions.Add(new Vector3(
                        ParseFloat(parts[1]),
                        ParseFloat(parts[2]),
                        ParseFloat(parts[3])));
                    break;

                case "vt":
                    texCoords.Add(new Vector2(
                        ParseFloat(parts[1]),
                        1.0f - ParseFloat(parts[2])));
                    break;

                case "vn":
                    normals.Add(new Vector3(
                        ParseFloat(parts[1]),
                        ParseFloat(parts[2]),
                        ParseFloat(parts[3])));
                    break;

                case "f":
                    if (parts.Length < 4)
                        throw new NotSupportedException("Face must have at least 3 vertices.");

                    for (int i = 2; i < parts.Length - 1; i++)
                    {
                        vertices.Add(ParseFaceVertex(parts[1], positions, texCoords, normals));
                        vertices.Add(ParseFaceVertex(parts[i], positions, texCoords, normals));
                        vertices.Add(ParseFaceVertex(parts[i + 1], positions, texCoords, normals));
                    }
                    break;

                    vertices.Add(ParseFaceVertex(parts[1], positions, texCoords, normals));
                    vertices.Add(ParseFaceVertex(parts[2], positions, texCoords, normals));
                    vertices.Add(ParseFaceVertex(parts[3], positions, texCoords, normals));
                    break;
            }
        }

        string? diffuseTexture = null;

        if (!string.IsNullOrWhiteSpace(mtlFileName))
        {
            string mtlPath = Path.Combine(objDir, mtlFileName);
            if (File.Exists(mtlPath))
                diffuseTexture = LoadDiffuseTextureFromMtl(mtlPath, currentMaterial);
        }

        if (!string.IsNullOrWhiteSpace(diffuseTexture))
            diffuseTexture = Path.Combine(objDir, diffuseTexture);

        return new ObjMesh(vertices.ToArray(), diffuseTexture);
    }

    private static ObjVertex ParseFaceVertex(
        string token,
        List<Vector3> positions,
        List<Vector2> texCoords,
        List<Vector3> normals)
    {
        string[] idx = token.Split('/');

        int pi = ParseIndex(idx[0], positions.Count);

        Vector2 uv = Vector2.Zero;
        Vector3 normal = Vector3.UnitZ; // fallback

        if (idx.Length > 1 && idx[1].Length > 0)
        {
            int ti = ParseIndex(idx[1], texCoords.Count);
            uv = texCoords[ti];
        }

        if (idx.Length > 2 && idx[2].Length > 0)
        {
            int ni = ParseIndex(idx[2], normals.Count);
            normal = normals[ni];
        }

        return new ObjVertex(
            positions[pi],
            uv,
            normal);
    }

    private static int ParseIndex(string s, int count)
    {
        int index = int.Parse(s, CultureInfo.InvariantCulture);

        if (index > 0)
            return index - 1;

        if (index < 0)
            return count + index;

        throw new FormatException("OBJ index 0 is invalid.");
    }

    private static string? LoadDiffuseTextureFromMtl(string mtlPath, string? targetMaterial)
    {
        string? currentMaterial = null;
        bool inTarget = targetMaterial == null;

        foreach (string rawLine in File.ReadAllLines(mtlPath))
        {
            string line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#"))
                continue;

            string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            switch (parts[0])
            {
                case "newmtl":
                    currentMaterial = parts.Length >= 2 ? parts[1] : null;
                    inTarget = targetMaterial == null || currentMaterial == targetMaterial;
                    break;

                case "map_Kd":
                    if (inTarget && parts.Length >= 2)
                        return parts[1];
                    break;
            }
        }

        return null;
    }

    private static float ParseFloat(string s) =>
        float.Parse(s, CultureInfo.InvariantCulture);
}