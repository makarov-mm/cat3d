namespace Cat3d;

public sealed class ObjMesh
{
    public ObjVertex[] Vertices { get; }
    public string? DiffuseTextureFile { get; }

    public ObjMesh(ObjVertex[] vertices, string? diffuseTextureFile)
    {
        Vertices = vertices;
        DiffuseTextureFile = diffuseTextureFile;
    }
}