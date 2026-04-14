using System.Runtime.InteropServices;

namespace Cat3d;

internal static class Native
{
    public const int CS_OWNDC = 0x0020;
    public const int WS_OVERLAPPEDWINDOW = 0x00CF0000;

    public const byte PFD_TYPE_RGBA = 0;
    public const sbyte PFD_MAIN_PLANE = 0;
    public const uint PFD_DOUBLEBUFFER = 0x00000001;
    public const uint PFD_SUPPORT_OPENGL = 0x00000020;
    public const uint PFD_DRAW_TO_WINDOW = 0x00000004;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPStr)] public string? lpszMenuName;
        [MarshalAs(UnmanagedType.LPStr)] public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits;
        public byte cRedShift;
        public byte cGreenBits;
        public byte cGreenShift;
        public byte cBlueBits;
        public byte cBlueShift;
        public byte cAlphaBits;
        public byte cAlphaShift;
        public byte cAccumBits;
        public byte cAccumRedBits;
        public byte cAccumGreenBits;
        public byte cAccumBlueBits;
        public byte cAccumAlphaBits;
        public byte cDepthBits;
        public byte cStencilBits;
        public byte cAuxBuffers;
        public sbyte iLayerType;
        public byte bReserved;
        public uint dwLayerMask;
        public uint dwVisibleMask;
        public uint dwDamageMask;
    }

    [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern IntPtr CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int X,
        int Y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "DefWindowProcA", CharSet = CharSet.Ansi)]
    public static extern IntPtr DefWindowProcA(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public static readonly WndProc DefWindowProcDelegate = DefWindowProcA;

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    public static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR ppfd);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool SetPixelFormat(IntPtr hdc, int format, ref PIXELFORMATDESCRIPTOR ppfd);

    [DllImport("gdi32.dll")]
    public static extern bool SwapBuffers(IntPtr hdc);

    [DllImport("opengl32.dll")]
    public static extern IntPtr wglCreateContext(IntPtr hdc);

    [DllImport("opengl32.dll")]
    public static extern bool wglDeleteContext(IntPtr hglrc);

    [DllImport("opengl32.dll")]
    public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

    [DllImport("opengl32.dll", CharSet = CharSet.Ansi)]
    public static extern IntPtr wglGetProcAddress(string lpszProc);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    public static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    public static IntPtr GetGLProcAddress(string name)
    {
        IntPtr p = wglGetProcAddress(name);
        if (p == IntPtr.Zero || p == (IntPtr)1 || p == (IntPtr)2 || p == (IntPtr)3 || p == (IntPtr)(-1))
        {
            IntPtr mod = LoadLibrary("opengl32.dll");
            p = GetProcAddress(mod, name);
        }
        return p;
    }
}