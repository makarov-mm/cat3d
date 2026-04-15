namespace Cat3d;

public static class Files
{
    public static string Model => Path.Combine(AppContext.BaseDirectory, "objects", "12221_Cat_v1_l3.obj");
    public static string TextureDiffuse => Path.Combine(AppContext.BaseDirectory, "objects", "Cat_diffuse.jpg");
    public static string TextureBump => Path.Combine(AppContext.BaseDirectory, "objects", "Cat_bump.jpg");
    public static string ShaderVertex => Path.Combine(AppContext.BaseDirectory, "shaders", "cat.vert");
    public static string ShaderFragment => Path.Combine(AppContext.BaseDirectory, "shaders", "cat.frag");
}