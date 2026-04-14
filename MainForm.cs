using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace Cat3d;

public sealed class MainForm : Form
{
    private readonly Timer _timer;

    private IntPtr _hdc;
    private IntPtr _hglrc;
    private bool _glInitialized;

    private bool _mouseDown;
    private Point _lastMouse;

    private float _rotX = -3.14f / 2;
    private float _rotY = 0.0f;
    private float _autoRotY = 0.0f;
    private float _zoom = 70f;

    private long _lastTicks;

    private uint _vao;
    private uint _vbo;
    private uint _program;

    private ObjMesh _mesh;
    private uint _catDiffuse;
    private uint _catBump;

    private int _uModel = -1;
    private int _uView = -1;
    private int _uProj = -1;
    private int _uDiffuse = -1;
    private int _uNormalMap = -1;
    private int _uLightDir = -1;
    private int _uViewPos = -1;
    private int _uInvertNormalY = -1;

    private const bool InvertNormalMapGreenChannel = false;

    public MainForm()
    {
        Text = "WinForms + OpenGL Core + Normal Mapping";
        ClientSize = new Size(800, 800);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

        _timer = new Timer { Interval = 16 };
        _timer.Tick += (_, _) => RenderFrame();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ClassStyle |= Native.CS_OWNDC;
            return cp;
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        try
        {
            InitOpenGL();
            InitScene();
            _glInitialized = true;
            _lastTicks = Stopwatch.GetTimestamp();
            _timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.ToString(), "Initialization error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _timer.Stop();
        DestroyScene();

        if (_hglrc != IntPtr.Zero)
        {
            Native.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            Native.wglDeleteContext(_hglrc);
            _hglrc = IntPtr.Zero;
        }

        if (_hdc != IntPtr.Zero)
        {
            Native.ReleaseDC(Handle, _hdc);
            _hdc = IntPtr.Zero;
        }

        base.OnFormClosing(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_glInitialized)
            RenderFrame();
        else
            e.Graphics.Clear(Color.Black);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            Close();

        base.OnKeyDown(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _mouseDown = true;
            _lastMouse = e.Location;
            Capture = true;
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _mouseDown = false;
            Capture = false;
        }

        base.OnMouseUp(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_mouseDown)
        {
            int dx = e.X - _lastMouse.X;
            int dy = e.Y - _lastMouse.Y;

            _rotY += dx * 0.01f;
            _rotX += dy * 0.01f;
            _rotX = Math.Clamp(_rotX, -1.5f, 1.5f);

            _lastMouse = e.Location;
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        _zoom -= Math.Sign(e.Delta) * 0.35f;
        _zoom = Math.Clamp(_zoom, 60f, 120f);

        base.OnMouseWheel(e);
    }

    private void InitOpenGL()
    {
        _hdc = Native.GetDC(Handle);
        if (_hdc == IntPtr.Zero)
            throw new Win32Exception("GetDC failed.");

        SetBasicPixelFormat(_hdc);
        InitWGLExtensions();

        _hglrc = CreateModernContext(_hdc);
        if (_hglrc == IntPtr.Zero)
            throw new Exception("Failed to create modern OpenGL context.");

        if (!Native.wglMakeCurrent(_hdc, _hglrc))
            throw new Win32Exception("wglMakeCurrent failed.");

        GL.LoadAll();
    }

    private void InitWGLExtensions()
    {
        string cls = "Dummy_WGL_Window";

        var wc = new Native.WNDCLASS
        {
            style = Native.CS_OWNDC,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(Native.DefWindowProcDelegate),
            hInstance = Native.GetModuleHandle(null),
            lpszClassName = cls
        };

        ushort atom = Native.RegisterClass(ref wc);
        if (atom == 0)
        {
            int err = Marshal.GetLastWin32Error();
            if (err != 1410) // ERROR_CLASS_ALREADY_EXISTS
                throw new Win32Exception(err);
        }

        IntPtr hwnd = Native.CreateWindowEx(
            0,
            cls,
            "dummy",
            Native.WS_OVERLAPPEDWINDOW,
            0, 0, 1, 1,
            IntPtr.Zero,
            IntPtr.Zero,
            wc.hInstance,
            IntPtr.Zero);

        if (hwnd == IntPtr.Zero)
            throw new Win32Exception("CreateWindowEx dummy failed.");

        IntPtr hdc = Native.GetDC(hwnd);
        if (hdc == IntPtr.Zero)
        {
            Native.DestroyWindow(hwnd);
            throw new Win32Exception("GetDC dummy failed.");
        }

        try
        {
            SetBasicPixelFormat(hdc);

            IntPtr rc = Native.wglCreateContext(hdc);
            if (rc == IntPtr.Zero)
                throw new Win32Exception("wglCreateContext dummy failed.");

            try
            {
                if (!Native.wglMakeCurrent(hdc, rc))
                    throw new Win32Exception("wglMakeCurrent dummy failed.");

                WGL.LoadExtensions();
            }
            finally
            {
                Native.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                Native.wglDeleteContext(rc);
            }
        }
        finally
        {
            Native.ReleaseDC(hwnd, hdc);
            Native.DestroyWindow(hwnd);
        }
    }

    private unsafe IntPtr CreateModernContext(IntPtr hdc)
    {
        int[][] versions =
        {
            new[] { 4, 6 },
            new[] { 4, 5 },
            new[] { 4, 4 },
            new[] { 4, 3 },
            new[] { 4, 2 },
            new[] { 4, 1 },
            new[] { 4, 0 },
            new[] { 3, 3 },
        };

        foreach (var v in versions)
        {
            int[] attribs =
            {
                WGL.WGL_CONTEXT_MAJOR_VERSION_ARB, v[0],
                WGL.WGL_CONTEXT_MINOR_VERSION_ARB, v[1],
                WGL.WGL_CONTEXT_PROFILE_MASK_ARB, WGL.WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
                WGL.WGL_CONTEXT_FLAGS_ARB, 0,
                0
            };

            fixed (int* pAttribs = attribs)
            {
                IntPtr rc = WGL.wglCreateContextAttribsARB(hdc, IntPtr.Zero, (IntPtr)pAttribs);
                if (rc != IntPtr.Zero)
                    return rc;
            }
        }

        return IntPtr.Zero;
    }

    private void SetBasicPixelFormat(IntPtr hdc)
    {
        var pfd = new Native.PIXELFORMATDESCRIPTOR
        {
            nSize = (ushort)Marshal.SizeOf<Native.PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            dwFlags = Native.PFD_DRAW_TO_WINDOW | Native.PFD_SUPPORT_OPENGL | Native.PFD_DOUBLEBUFFER,
            iPixelType = Native.PFD_TYPE_RGBA,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
            iLayerType = Native.PFD_MAIN_PLANE
        };

        int pf = Native.ChoosePixelFormat(hdc, ref pfd);
        if (pf == 0)
            throw new Win32Exception("ChoosePixelFormat failed.");

        if (!Native.SetPixelFormat(hdc, pf, ref pfd))
            throw new Win32Exception("SetPixelFormat failed.");
    }

    private void InitScene()
    {
        _mesh = ObjLoader.Load("..\\..\\..\\..\\objects\\12221_Cat_v1_l3.obj");
        _catDiffuse = Texture.LoadTextureFromFile("..\\..\\..\\..\\objects\\Cat_diffuse.jpg");
        _catBump = Texture.LoadTextureFromFile("..\\..\\..\\..\\objects\\Cat_bump.jpg");

        _program = CreateProgram();

        _uModel = GL.GetUniformLocation(_program, "uModel");
        _uView = GL.GetUniformLocation(_program, "uView");
        _uProj = GL.GetUniformLocation(_program, "uProj");
        _uDiffuse = GL.GetUniformLocation(_program, "uDiffuse");
        _uNormalMap = GL.GetUniformLocation(_program, "uNormalMap");
        _uLightDir = GL.GetUniformLocation(_program, "uLightDir");
        _uViewPos = GL.GetUniformLocation(_program, "uViewPos");
        _uInvertNormalY = GL.GetUniformLocation(_program, "uInvertNormalY");

        CreateObject();

        GL.Enable(GL.GL_DEPTH_TEST);
        GL.Enable(GL.GL_CULL_FACE);
        GL.CullFace(GL.GL_BACK);
        GL.ClearColor(0.8f, 0.8f, 0.8f, 1.0f);

        if (WGL.wglSwapIntervalEXT != null)
            WGL.wglSwapIntervalEXT(1);
    }

    private void DestroyScene()
    {
        GL.DeleteTextures(1, ref _catDiffuse);
        GL.DeleteTextures(1, ref _catBump);

        if (_vbo != 0)
        {
            GL.DeleteBuffers(1, ref _vbo);
            _vbo = 0;
        }

        if (_vao != 0)
        {
            GL.DeleteVertexArrays(1, ref _vao);
            _vao = 0;
        }

        if (_program != 0)
        {
            GL.DeleteProgram(_program);
            _program = 0;
        }
    }

    private uint CreateProgram()
    {
        const string vs = """
#version 460 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUV;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in vec3 aTangent;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;

out vec2 vUV;
out vec3 vWorldPos;
out mat3 vTBN;

void main()
{
    vec3 worldPos = vec3(uModel * vec4(aPos, 1.0));

    mat3 normalMatrix = mat3(transpose(inverse(uModel)));

    vec3 N = normalize(normalMatrix * aNormal);
    vec3 T = normalize(normalMatrix * aTangent);
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);

    vUV = aUV;
    vWorldPos = worldPos;
    vTBN = mat3(T, B, N);

    gl_Position = uProj * uView * vec4(worldPos, 1.0);
}
""";

        const string fs = """
#version 460 core

in vec2 vUV;
in vec3 vWorldPos;
in mat3 vTBN;

out vec4 FragColor;

uniform sampler2D uDiffuse;
uniform sampler2D uNormalMap;
uniform vec3 uLightDir;
uniform vec3 uViewPos;
uniform int uInvertNormalY;

void main()
{
    vec3 albedo = texture(uDiffuse, vUV).rgb;

    vec3 normalTS = texture(uNormalMap, vUV).rgb * 2.0 - 1.0;
    if (uInvertNormalY != 0)
        normalTS.y = -normalTS.y;

    normalTS = normalize(normalTS);

    vec3 N = normalize(vTBN * vec3(0,0,1));
    vec3 L = normalize(-uLightDir);
    vec3 V = normalize(uViewPos - vWorldPos);
    vec3 H = normalize(L + V);

    float ambientStrength = 0.18;
    vec3 ambient = albedo * ambientStrength;

    float diff = max(dot(N, L), 0.0);
    vec3 diffuse = albedo * diff;

    float specPower = 24.0;
    float specStrength = 0.15;
    
    //float specPower = 64.0;
    //float specStrength = 0.35;
    
    float spec = pow(max(dot(N, H), 0.0), specPower);
    vec3 specular = vec3(spec * specStrength);

    vec3 color = ambient + diffuse + specular;

    //color = pow(color, vec3(1.0 / 2.2));
    FragColor = vec4(color, 1.0);
}
""";

        uint vert = CompileShader(GL.GL_VERTEX_SHADER, vs);
        uint frag = CompileShader(GL.GL_FRAGMENT_SHADER, fs);

        uint program = GL.CreateProgram();
        GL.AttachShader(program, vert);
        GL.AttachShader(program, frag);
        GL.LinkProgram(program);

        GL.DeleteShader(vert);
        GL.DeleteShader(frag);

        GL.GetProgramiv(program, GL.GL_LINK_STATUS, out int ok);
        if (ok == 0)
            throw new Exception("Program link failed:\n" + GL.GetProgramInfoLog(program));

        return program;
    }

    private uint CompileShader(uint type, string source)
    {
        uint shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShaderiv(shader, GL.GL_COMPILE_STATUS, out int ok);
        if (ok == 0)
            throw new Exception("Shader compilation failed:\n" + GL.GetShaderInfoLog(shader));

        return shader;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        public float px, py, pz;
        public float u, v;
        public float nx, ny, nz;
        public float tx, ty, tz;

        public Vertex(
            float px, float py, float pz,
            float u, float v,
            float nx, float ny, float nz,
            float tx, float ty, float tz)
        {
            this.px = px; this.py = py; this.pz = pz;
            this.u = u; this.v = v;
            this.nx = nx; this.ny = ny; this.nz = nz;
            this.tx = tx; this.ty = ty; this.tz = tz;
        }
    }

    private unsafe void CreateObject()
    {
        Vertex[] vertices = _mesh.Vertices
            .Select(vx => new Vertex(
                vx.Position.X, vx.Position.Y, vx.Position.Z, 
                vx.TexCoord.X, vx.TexCoord.Y, 
                vx.Normal.X, vx.Normal.Y, vx.Normal.Z,
                vx.Tangent.X, vx.Tangent.Y, vx.Tangent.Z))
            .ToArray();

        GL.GenVertexArrays(1, out _vao);
        GL.BindVertexArray(_vao);

        GL.GenBuffers(1, out _vbo);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);

        fixed (Vertex* p = vertices)
        {
            GL.BufferData(GL.GL_ARRAY_BUFFER, (nint)(vertices.Length * sizeof(Vertex)), (IntPtr)p, GL.GL_STATIC_DRAW);
        }

        int stride = sizeof(Vertex);

        GL.VertexAttribPointer(0, 3, GL.GL_FLOAT, false, stride, Marshal.OffsetOf<Vertex>(nameof(Vertex.px)));
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, GL.GL_FLOAT, false, stride, Marshal.OffsetOf<Vertex>(nameof(Vertex.u)));
        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(2, 3, GL.GL_FLOAT, false, stride, Marshal.OffsetOf<Vertex>(nameof(Vertex.nx)));
        GL.EnableVertexAttribArray(2);

        GL.VertexAttribPointer(3, 3, GL.GL_FLOAT, false, stride, Marshal.OffsetOf<Vertex>(nameof(Vertex.tx)));
        GL.EnableVertexAttribArray(3);

        GL.BindVertexArray(0);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, 0);
    }

    private unsafe void RenderFrame()
    {
        if (!_glInitialized || IsDisposed || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;

        long now = Stopwatch.GetTimestamp();
        float dt = (float)(now - _lastTicks) / Stopwatch.Frequency;
        _lastTicks = now;

        _autoRotY += dt * 0.8f;

        GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
        GL.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

        float aspect = (float)ClientSize.Width / ClientSize.Height;

        Mat4 proj = Mat4.Perspective(60.0f * MathF.PI / 180.0f, aspect, 0.1f, 100.0f);
        Mat4 view = Mat4.Translation(0.0f, 0.0f, -_zoom);
        Mat4 rotY = Mat4.RotationY(_rotY + _autoRotY);
        Mat4 rotX = Mat4.RotationX(_rotX);
        Mat4 translation = Mat4.Translation(0, -20, 0.0f);
        Mat4 rotation = Mat4.Multiply(rotY, rotX);
        Mat4 model = Mat4.Multiply(translation, rotation);

        GL.UseProgram(_program);

        fixed (float* pModel = model.M)
            GL.UniformMatrix4fv(_uModel, 1, false, (IntPtr)pModel);

        fixed (float* pView = view.M)
            GL.UniformMatrix4fv(_uView, 1, false, (IntPtr)pView);

        fixed (float* pProj = proj.M)
            GL.UniformMatrix4fv(_uProj, 1, false, (IntPtr)pProj);

        if (dir)
        {
            xxx += 0.01f;
        }
        else
        {
            xxx -= 0.01f;
        }

        if (dir && xxx > 2)
        {
            dir = false;
        }
        else if (!dir && xxx < -2)
        {
            dir = true;
        }

        GL.Uniform3f(_uLightDir, xxx, -0.0f, -2.0f);
        GL.Uniform3f(_uViewPos, 0.0f, 0.0f, _zoom);
        GL.Uniform1i(_uInvertNormalY, InvertNormalMapGreenChannel ? 1 : 0);

        GL.ActiveTexture(GL.GL_TEXTURE0);
        GL.BindTexture(GL.GL_TEXTURE_2D, _catDiffuse);
        GL.Uniform1i(_uDiffuse, 0);

        GL.ActiveTexture(GL.GL_TEXTURE1);
        GL.BindTexture(GL.GL_TEXTURE_2D, _catBump);
        GL.Uniform1i(_uNormalMap, 1);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(GL.GL_TRIANGLES, 0, _mesh.Vertices.Length);
        GL.BindVertexArray(0);

        Native.SwapBuffers(_hdc);
    }

    private static bool dir;
    private static float xxx = -2;
}