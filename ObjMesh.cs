using System.Numerics;

namespace Cat3d;

// An indexed triangle mesh with the texture files of its material (null for a map the
// material doesn't have).
internal sealed class ObjMesh
{
    public ObjMesh(ObjVertex[] vertices, uint[] indices, string? diffuseMap, string? bumpMap)
    {
        Vertices = vertices;
        Indices = indices;
        DiffuseMap = diffuseMap;
        BumpMap = bumpMap;

        Vector3 min = new(float.PositiveInfinity), max = new(float.NegativeInfinity);

        foreach (ObjVertex vertex in vertices)
        {
            min = Vector3.Min(min, vertex.Position);
            max = Vector3.Max(max, vertex.Position);
        }

        Center = (min + max) / 2;

        foreach (ObjVertex vertex in vertices)
        {
            Radius = MathF.Max(Radius, Vector3.Distance(vertex.Position, Center));
        }
    }

    public ObjVertex[] Vertices { get; }
    public uint[] Indices { get; } // three per triangle, counterclockwise in front
    public string? DiffuseMap { get; }
    public string? BumpMap { get; } // a height map

    // The middle of the bounding box, and the radius of the sphere around it that holds the mesh.
    public Vector3 Center { get; }
    public float Radius { get; }
}
