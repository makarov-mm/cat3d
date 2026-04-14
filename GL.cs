using System.Runtime.InteropServices;

namespace Cat3d;

internal static class GL
{
    public const uint GL_ARRAY_BUFFER = 0x8892;
    public const uint GL_STATIC_DRAW = 0x88E4;
    public const uint GL_FLOAT = 0x1406;
    public const uint GL_TRIANGLES = 0x0004;

    public const uint GL_VERTEX_SHADER = 0x8B31;
    public const uint GL_FRAGMENT_SHADER = 0x8B30;
    public const uint GL_COMPILE_STATUS = 0x8B81;
    public const uint GL_LINK_STATUS = 0x8B82;
    public const uint GL_INFO_LOG_LENGTH = 0x8B84;

    public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
    public const uint GL_DEPTH_BUFFER_BIT = 0x00000100;
    public const uint GL_DEPTH_TEST = 0x0B71;
    public const uint GL_CULL_FACE = 0x0B44;
    public const uint GL_BACK = 0x0405;

    public const uint GL_TEXTURE0 = 0x84C0;
    public const uint GL_TEXTURE1 = 0x84C1;
    public const uint GL_TEXTURE_2D = 0x0DE1;
    public const int GL_TEXTURE_WRAP_S = 0x2802;
    public const int GL_TEXTURE_WRAP_T = 0x2803;
    public const int GL_TEXTURE_MIN_FILTER = 0x2801;
    public const int GL_TEXTURE_MAG_FILTER = 0x2800;
    public const int GL_REPEAT = 0x2901;
    public const int GL_LINEAR = 0x2601;
    public const int GL_LINEAR_MIPMAP_LINEAR = 0x2703;
    public const int GL_RGBA8 = 0x8058;
    public const uint GL_RGBA = 0x1908;
    public const uint GL_UNSIGNED_BYTE = 0x1401;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glGenVertexArraysProc(int n, out uint arrays);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glBindVertexArrayProc(uint array);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glDeleteVertexArraysProc(int n, ref uint arrays);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glGenBuffersProc(int n, out uint buffers);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glBindBufferProc(uint target, uint buffer);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glBufferDataProc(uint target, nint size, IntPtr data, uint usage);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glDeleteBuffersProc(int n, ref uint buffers);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glVertexAttribPointerProc(uint index, int size, uint type, byte normalized, int stride, IntPtr pointer);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glEnableVertexAttribArrayProc(uint index);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate uint glCreateShaderProc(uint type);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glShaderSourceProc(uint shader, int count, IntPtr strings, IntPtr lengths);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glCompileShaderProc(uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glGetShaderivProc(uint shader, uint pname, out int param);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glGetShaderInfoLogProc(uint shader, int maxLength, out int length, IntPtr infoLog);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glDeleteShaderProc(uint shader);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate uint glCreateProgramProc();
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glAttachShaderProc(uint program, uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glLinkProgramProc(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glGetProgramivProc(uint program, uint pname, out int param);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glGetProgramInfoLogProc(uint program, int maxLength, out int length, IntPtr infoLog);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glUseProgramProc(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glDeleteProgramProc(uint program);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate int glGetUniformLocationProc(uint program, [MarshalAs(UnmanagedType.LPStr)] string name);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glUniformMatrix4fvProc(int location, int count, byte transpose, IntPtr value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glUniform1iProc(int location, int v0);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glUniform3fProc(int location, float v0, float v1, float v2);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glActiveTextureProc(uint texture);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] public delegate void glGenerateMipmapProc(uint target);

    public static glGenVertexArraysProc _GenVertexArrays = null!;
    public static glBindVertexArrayProc _BindVertexArray = null!;
    public static glDeleteVertexArraysProc _DeleteVertexArrays = null!;
    public static glGenBuffersProc _GenBuffers = null!;
    public static glBindBufferProc _BindBuffer = null!;
    public static glBufferDataProc _BufferData = null!;
    public static glDeleteBuffersProc _DeleteBuffers = null!;
    public static glVertexAttribPointerProc _VertexAttribPointer = null!;
    public static glEnableVertexAttribArrayProc _EnableVertexAttribArray = null!;
    public static glCreateShaderProc _CreateShader = null!;
    public static glShaderSourceProc _ShaderSource = null!;
    public static glCompileShaderProc _CompileShader = null!;
    public static glGetShaderivProc _GetShaderiv = null!;
    public static glGetShaderInfoLogProc _GetShaderInfoLog = null!;
    public static glDeleteShaderProc _DeleteShader = null!;
    public static glCreateProgramProc _CreateProgram = null!;
    public static glAttachShaderProc _AttachShader = null!;
    public static glLinkProgramProc _LinkProgram = null!;
    public static glGetProgramivProc _GetProgramiv = null!;
    public static glGetProgramInfoLogProc _GetProgramInfoLog = null!;
    public static glUseProgramProc _UseProgram = null!;
    public static glDeleteProgramProc _DeleteProgram = null!;
    public static glGetUniformLocationProc _GetUniformLocation = null!;
    public static glUniformMatrix4fvProc _UniformMatrix4fv = null!;
    public static glUniform1iProc _Uniform1i = null!;
    public static glUniform3fProc _Uniform3f = null!;
    public static glActiveTextureProc _ActiveTexture = null!;
    public static glGenerateMipmapProc _GenerateMipmap = null!;

    [DllImport("opengl32.dll")] public static extern void glClearColor(float r, float g, float b, float a);
    [DllImport("opengl32.dll")] public static extern void glClear(uint mask);
    [DllImport("opengl32.dll")] public static extern void glViewport(int x, int y, int w, int h);
    [DllImport("opengl32.dll")] public static extern void glEnable(uint cap);
    [DllImport("opengl32.dll")] public static extern void glCullFace(uint mode);
    [DllImport("opengl32.dll")] public static extern void glDrawArrays(uint mode, int first, int count);
    [DllImport("opengl32.dll")] public static extern void glGenTextures(int n, out uint textures);
    [DllImport("opengl32.dll")] public static extern void glDeleteTextures(int n, ref uint textures);
    [DllImport("opengl32.dll")] public static extern void glBindTexture(uint target, uint texture);
    [DllImport("opengl32.dll")] public static extern void glTexParameteri(uint target, int pname, int param);
    [DllImport("opengl32.dll")] public static extern void glTexImage2D(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, IntPtr pixels);

    public static void GenVertexArrays(int n, out uint vao) => _GenVertexArrays(n, out vao);
    public static void BindVertexArray(uint vao) => _BindVertexArray(vao);
    public static void DeleteVertexArrays(int n, ref uint vao) => _DeleteVertexArrays(n, ref vao);
    public static void GenBuffers(int n, out uint vbo) => _GenBuffers(n, out vbo);
    public static void BindBuffer(uint target, uint buffer) => _BindBuffer(target, buffer);
    public static void BufferData(uint target, nint size, IntPtr data, uint usage) => _BufferData(target, size, data, usage);
    public static void DeleteBuffers(int n, ref uint vbo) => _DeleteBuffers(n, ref vbo);
    public static void VertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, IntPtr pointer) => _VertexAttribPointer(index, size, type, normalized ? (byte)1 : (byte)0, stride, pointer);
    public static void EnableVertexAttribArray(uint index) => _EnableVertexAttribArray(index);
    public static uint CreateShader(uint type) => _CreateShader(type);
    public static void CompileShader(uint shader) => _CompileShader(shader);
    public static void GetShaderiv(uint shader, uint pname, out int param) => _GetShaderiv(shader, pname, out param);
    public static void DeleteShader(uint shader) => _DeleteShader(shader);
    public static uint CreateProgram() => _CreateProgram();
    public static void AttachShader(uint program, uint shader) => _AttachShader(program, shader);
    public static void LinkProgram(uint program) => _LinkProgram(program);
    public static void GetProgramiv(uint program, uint pname, out int param) => _GetProgramiv(program, pname, out param);
    public static void UseProgram(uint program) => _UseProgram(program);
    public static void DeleteProgram(uint program) => _DeleteProgram(program);
    public static int GetUniformLocation(uint program, string name) => _GetUniformLocation(program, name);
    public static void UniformMatrix4fv(int location, int count, bool transpose, IntPtr value) => _UniformMatrix4fv(location, count, transpose ? (byte)1 : (byte)0, value);
    public static void Uniform1i(int location, int v0) => _Uniform1i(location, v0);
    public static void Uniform3f(int location, float x, float y, float z) => _Uniform3f(location, x, y, z);
    public static void ActiveTexture(uint tex) => _ActiveTexture(tex);
    public static void GenerateMipmap(uint target) => _GenerateMipmap(target);
    public static void ClearColor(float r, float g, float b, float a) => glClearColor(r, g, b, a);
    public static void Clear(uint mask) => glClear(mask);
    public static void Viewport(int x, int y, int w, int h) => glViewport(x, y, w, h);
    public static void Enable(uint cap) => glEnable(cap);
    public static void CullFace(uint mode) => glCullFace(mode);
    public static void DrawArrays(uint mode, int first, int count) => glDrawArrays(mode, first, count);
    public static void GenTextures(int n, out uint textures) => glGenTextures(n, out textures);
    public static void DeleteTextures(int n, ref uint textures) => glDeleteTextures(n, ref textures);
    public static void BindTexture(uint target, uint texture) => glBindTexture(target, texture);
    public static void TexParameteri(uint target, int pname, int param) => glTexParameteri(target, pname, param);
    public static void TexImage2D(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, IntPtr pixels)
        => glTexImage2D(target, level, internalFormat, width, height, border, format, type, pixels);

    public static void LoadAll()
    {
        _GenVertexArrays = Load<glGenVertexArraysProc>("glGenVertexArrays");
        _BindVertexArray = Load<glBindVertexArrayProc>("glBindVertexArray");
        _DeleteVertexArrays = Load<glDeleteVertexArraysProc>("glDeleteVertexArrays");
        _GenBuffers = Load<glGenBuffersProc>("glGenBuffers");
        _BindBuffer = Load<glBindBufferProc>("glBindBuffer");
        _BufferData = Load<glBufferDataProc>("glBufferData");
        _DeleteBuffers = Load<glDeleteBuffersProc>("glDeleteBuffers");
        _VertexAttribPointer = Load<glVertexAttribPointerProc>("glVertexAttribPointer");
        _EnableVertexAttribArray = Load<glEnableVertexAttribArrayProc>("glEnableVertexAttribArray");
        _CreateShader = Load<glCreateShaderProc>("glCreateShader");
        _ShaderSource = Load<glShaderSourceProc>("glShaderSource");
        _CompileShader = Load<glCompileShaderProc>("glCompileShader");
        _GetShaderiv = Load<glGetShaderivProc>("glGetShaderiv");
        _GetShaderInfoLog = Load<glGetShaderInfoLogProc>("glGetShaderInfoLog");
        _DeleteShader = Load<glDeleteShaderProc>("glDeleteShader");
        _CreateProgram = Load<glCreateProgramProc>("glCreateProgram");
        _AttachShader = Load<glAttachShaderProc>("glAttachShader");
        _LinkProgram = Load<glLinkProgramProc>("glLinkProgram");
        _GetProgramiv = Load<glGetProgramivProc>("glGetProgramiv");
        _GetProgramInfoLog = Load<glGetProgramInfoLogProc>("glGetProgramInfoLog");
        _UseProgram = Load<glUseProgramProc>("glUseProgram");
        _DeleteProgram = Load<glDeleteProgramProc>("glDeleteProgram");
        _GetUniformLocation = Load<glGetUniformLocationProc>("glGetUniformLocation");
        _UniformMatrix4fv = Load<glUniformMatrix4fvProc>("glUniformMatrix4fv");
        _Uniform1i = Load<glUniform1iProc>("glUniform1i");
        _Uniform3f = Load<glUniform3fProc>("glUniform3f");
        _ActiveTexture = Load<glActiveTextureProc>("glActiveTexture");
        _GenerateMipmap = Load<glGenerateMipmapProc>("glGenerateMipmap");
    }

    private static T Load<T>(string name) where T : Delegate
    {
        IntPtr p = Native.GetGLProcAddress(name);
        
        return p == IntPtr.Zero 
            ? throw new Exception($"Failed to load GL function {name}") 
            : Marshal.GetDelegateForFunctionPointer<T>(p);
    }

    public static unsafe void ShaderSource(uint shader, string source)
    {
        IntPtr srcPtr = Marshal.StringToHGlobalAnsi(source);
        
        try
        {
            IntPtr* pp = stackalloc IntPtr[1];
            pp[0] = srcPtr;
            _ShaderSource(shader, 1, (IntPtr)pp, IntPtr.Zero);
        }
        finally
        {
            Marshal.FreeHGlobal(srcPtr);
        }
    }

    public static string GetShaderInfoLog(uint shader)
    {
        _GetShaderiv(shader, GL_INFO_LOG_LENGTH, out int len);
        
        if (len <= 1)
            return string.Empty;

        IntPtr mem = Marshal.AllocHGlobal(len);
        
        try
        {
            _GetShaderInfoLog(shader, len, out _, mem);
            return Marshal.PtrToStringAnsi(mem) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(mem);
        }
    }

    public static string GetProgramInfoLog(uint program)
    {
        _GetProgramiv(program, GL_INFO_LOG_LENGTH, out int len);
        
        if (len <= 1)
            return string.Empty;

        IntPtr mem = Marshal.AllocHGlobal(len);
        
        try
        {
            _GetProgramInfoLog(program, len, out _, mem);
            return Marshal.PtrToStringAnsi(mem) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(mem);
        }
    }
}