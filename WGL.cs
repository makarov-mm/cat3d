using System.Runtime.InteropServices;

namespace Cat3d;

internal static class WGL
{
    public const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
    public const int WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092;
    public const int WGL_CONTEXT_PROFILE_MASK_ARB = 0x9126;
    public const int WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
    public const int WGL_CONTEXT_FLAGS_ARB = 0x2094;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate IntPtr wglCreateContextAttribsARBProc(IntPtr hdc, IntPtr shareContext, IntPtr attribList);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate bool wglSwapIntervalEXTProc(int interval);

    public static wglCreateContextAttribsARBProc wglCreateContextAttribsARB = null!;
    public static wglSwapIntervalEXTProc? wglSwapIntervalEXT;

    public static void LoadExtensions()
    {
        wglCreateContextAttribsARB = LoadDelegate<wglCreateContextAttribsARBProc>("wglCreateContextAttribsARB");
        wglSwapIntervalEXT = TryLoadDelegate<wglSwapIntervalEXTProc>("wglSwapIntervalEXT");
    }

    private static T LoadDelegate<T>(string name) where T : Delegate
    {
        IntPtr p = Native.GetGLProcAddress(name);
        if (p == IntPtr.Zero)
            throw new Exception($"Failed to load {name}");
        return Marshal.GetDelegateForFunctionPointer<T>(p);
    }

    private static T? TryLoadDelegate<T>(string name) where T : Delegate
    {
        IntPtr p = Native.GetGLProcAddress(name);
        
        return p == IntPtr.Zero 
            ? null 
            : Marshal.GetDelegateForFunctionPointer<T>(p);
    }
}