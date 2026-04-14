using System.Numerics;

namespace Cat3d;

public struct ObjVertex(Vector3 position, Vector2 texCoord, Vector3 normal)
{
    public Vector3 Position = position;
    public Vector2 TexCoord = texCoord;
    public Vector3 Normal = normal;
    public Vector3 Tangent;
}