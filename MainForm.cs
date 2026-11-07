using System.Diagnostics;

namespace Cat3d;

internal sealed class MainForm : Form
{
    private const string Title = "WinForms + OpenGL Core + Bump Mapping";
    private const float SpinRadiansPerSecond = 0.8f;
    private const float DragRadiansPerPixel = 0.01f;   // at 96 dpi
    private const float ZoomPerWheelNotch = 0.9f;

    private static readonly string ModelPath = Path.Combine(AppContext.BaseDirectory, "objects", "12221_Cat_v1_l3.obj");

    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private TimeSpan _lastFrame;
    private Turntable _turntable = Turntable.Default;
    private bool _dragging;
    private Point _lastMouse;

    private GLContext? _gl;
    private ModelRenderer? _renderer;

    public MainForm()
    {
        Text = Title;
        ClientSize = LogicalToDeviceUnits(new Size(800, 800));
        StartPosition = FormStartPosition.CenterScreen;
        // OpenGL paints the whole client area, and again while the window is resized.
        SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int CS_OWNDC = 0x0020;   // OpenGL keeps using the window's device context
            CreateParams cp = base.CreateParams;
            cp.ClassStyle |= CS_OWNDC;
            return cp;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        try
        {
            ObjMesh mesh = ObjLoader.Load(ModelPath);
            _gl = new GLContext(Handle);
            _renderer = new ModelRenderer(mesh);
        }
        catch (Exception error)
        {
            _gl?.Dispose();
            _gl = null;
            MessageBox.Show(this, error.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            BeginInvoke(Close);
            return;
        }

        Text = $"{Title}   |   OpenGL {_gl.Version}";
        _lastFrame = _clock.Elapsed;
        Application.Idle += OnIdle;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Application.Idle -= OnIdle;
        _renderer?.Dispose();   // its OpenGL objects go while the context is still current
        _renderer = null;
        _gl?.Dispose();
        _gl = null;
        base.OnFormClosed(e);
    }

    // Renders frame after frame while there are no messages to handle; vsync paces it.
    private void OnIdle(object? sender, EventArgs e)
    {
        while (WindowState != FormWindowState.Minimized && !Native.HasPendingMessages())
            RenderFrame();
    }

    protected override void OnPaint(PaintEventArgs e) => RenderFrame();

    private void RenderFrame()
    {
        if (_gl is null || _renderer is null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;

        TimeSpan now = _clock.Elapsed;
        if (!_dragging)   // the model holds still under the mouse
            _turntable = _turntable.Rotate((float)(now - _lastFrame).TotalSeconds * SpinRadiansPerSecond, 0);
        _lastFrame = now;

        _renderer.Render(ClientSize.Width, ClientSize.Height, _turntable, (float)now.TotalSeconds);
        _gl.SwapBuffers();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragging = true;
            _lastMouse = e.Location;
            Capture = true;
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            Capture = false;   // OnMouseCaptureChanged ends the drag
        base.OnMouseUp(e);
    }

    // Also when another window takes the mouse away in the middle of a drag.
    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        _dragging = false;
        base.OnMouseCaptureChanged(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragging)
        {
            float radiansPerPixel = DragRadiansPerPixel * 96f / DeviceDpi;
            _turntable = _turntable.Rotate((e.X - _lastMouse.X) * radiansPerPixel, (e.Y - _lastMouse.Y) * radiansPerPixel);
            _lastMouse = e.Location;
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        _turntable = _turntable.Zoom(MathF.Pow(ZoomPerWheelNotch, e.Delta / (float)SystemInformation.MouseWheelScrollDelta));
        base.OnMouseWheel(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            Close();
        base.OnKeyDown(e);
    }
}
