using System.Drawing.Imaging;

namespace Cat3d;

// A repeating, mipmapped 2D texture.
internal sealed class Texture : IDisposable
{
    private Texture(Bitmap image, int internalFormat)
    {
        Handle = GL.GenTexture();
        GL.BindTexture(GL.GL_TEXTURE_2D, Handle);

        // GDI+ hands out the pixels as BGRA, top row first (the OBJ loader flips v to match).
        BitmapData pixels = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            GL.TexImage2D(GL.GL_TEXTURE_2D, 0, internalFormat, pixels.Width, pixels.Height, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, pixels.Scan0);
        }
        finally
        {
            image.UnlockBits(pixels);
        }

        GL.GenerateMipmap(GL.GL_TEXTURE_2D);
        GL.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
        GL.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);
        GL.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR_MIPMAP_LINEAR);
        GL.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);
        GL.BindTexture(GL.GL_TEXTURE_2D, 0);
    }

    public uint Handle { get; }

    // An image file in color, or as a grayscale one only its red channel (a height map, say).
    public static Texture Load(string path, bool grayscale = false)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Could not find file '{path}'.", path);
        }

        using var image = new Bitmap(path);
        return new Texture(image, grayscale ? GL.GL_R8 : GL.GL_RGBA8);
    }

    // A single texel of one color.
    public static Texture Solid(Color color)
    {
        using var image = new Bitmap(1, 1);
        image.SetPixel(0, 0, color);
        return new Texture(image, GL.GL_RGBA8);
    }

    public void Bind(int unit)
    {
        GL.ActiveTexture(GL.GL_TEXTURE0 + (uint)unit);
        GL.BindTexture(GL.GL_TEXTURE_2D, Handle);
    }

    public void Dispose() => GL.DeleteTexture(Handle);
}
