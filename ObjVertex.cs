using System.Numerics;
using System.Runtime.InteropServices;

namespace Cat3d;

// A mesh vertex, laid out as the vertex buffer holds it.
[StructLayout(LayoutKind.Sequential)]
internal readonly struct ObjVertex(Vector3 position, Vector3 normal, Vector2 texCoord)
{
    public readonly Vector3 Position = position;
    public readonly Vector3 Normal = normal;
    public readonly Vector2 TexCoord = texCoord;
}
