namespace Cat3d;

internal sealed class ShaderProgram : IDisposable
{
    // Throws InvalidOperationException with the driver's log when a shader doesn't build.
    public ShaderProgram(string vertexSource, string fragmentSource)
    {
        uint vertexShader = Compile(GL.GL_VERTEX_SHADER, vertexSource, "vertex");
        uint fragmentShader;
        try
        {
            fragmentShader = Compile(GL.GL_FRAGMENT_SHADER, fragmentSource, "fragment");
        }
        catch
        {
            GL.DeleteShader(vertexShader);
            throw;
        }

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);
        GL.DeleteShader(vertexShader); // released together with the program
        GL.DeleteShader(fragmentShader);

        if (GL.GetProgram(Handle, GL.GL_LINK_STATUS) == 0)
        {
            string log = GL.GetProgramInfoLog(Handle);
            GL.DeleteProgram(Handle);
            throw new InvalidOperationException($"The shader program failed to link:\n{log}");
        }
    }

    public uint Handle { get; }
    public int Uniform(string name) => GL.GetUniformLocation(Handle, name);
    public void Use() => GL.UseProgram(Handle);
    public void Dispose() => GL.DeleteProgram(Handle);

    private static uint Compile(uint type, string source, string kind)
    {
        uint shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        if (GL.GetShader(shader, GL.GL_COMPILE_STATUS) != 0)
        {
            return shader;
        }

        string log = GL.GetShaderInfoLog(shader);
        GL.DeleteShader(shader);
        throw new InvalidOperationException($"The {kind} shader failed to compile:\n{log}");
    }
}
