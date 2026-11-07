using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cat3d;

// An indexed triangle mesh in GPU buffers: position, normal and texture coordinates at
// attribute locations 0, 1 and 2.
internal sealed class GpuMesh : IDisposable
{
    private readonly uint _vertexArray;
    private readonly uint _vertexBuffer;
    private readonly uint _indexBuffer;
    private readonly int _indexCount;

    public GpuMesh(ReadOnlySpan<ObjVertex> vertices, ReadOnlySpan<uint> indices)
    {
        _indexCount = indices.Length;
        _vertexArray = GL.GenVertexArray();
        _vertexBuffer = GL.GenBuffer();
        _indexBuffer = GL.GenBuffer();

        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, _vertexBuffer);
        GL.BufferData(GL.GL_ARRAY_BUFFER, vertices, GL.GL_STATIC_DRAW);
        GL.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _indexBuffer);   // the vertex array keeps this binding
        GL.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, indices, GL.GL_STATIC_DRAW);

        int stride = Unsafe.SizeOf<ObjVertex>();
        Attribute(0, 3, stride, nameof(ObjVertex.Position));
        Attribute(1, 3, stride, nameof(ObjVertex.Normal));
        Attribute(2, 2, stride, nameof(ObjVertex.TexCoord));
        GL.BindVertexArray(0);
    }

    public void Draw()
    {
        GL.BindVertexArray(_vertexArray);
        GL.DrawElements(GL.GL_TRIANGLES, _indexCount);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_indexBuffer);
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteVertexArray(_vertexArray);
    }

    private static void Attribute(uint location, int size, int stride, string field)
    {
        GL.EnableVertexAttribArray(location);
        GL.VertexAttribPointer(location, size, stride, Marshal.OffsetOf<ObjVertex>(field));
    }
}
