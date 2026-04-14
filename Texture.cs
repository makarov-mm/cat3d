using System.Drawing.Imaging;

namespace Cat3d;

public static class Texture
{
    public static unsafe uint LoadTextureFromFile(string path)
    {
        using Bitmap original = new Bitmap(path);
        using Bitmap bmp = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);

        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.DrawImage(original, 0, 0, original.Width, original.Height);
        }

        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            byte[] rgba = new byte[bmp.Width * bmp.Height * 4];
            byte* srcBase = (byte*)data.Scan0;
            int dstIndex = 0;

            for (int y = 0; y < bmp.Height; y++)
            {
                byte* srcRow = srcBase + y * data.Stride;
                for (int x = 0; x < bmp.Width; x++)
                {
                    byte b = srcRow[x * 4 + 0];
                    byte g = srcRow[x * 4 + 1];
                    byte r = srcRow[x * 4 + 2];
                    byte a = srcRow[x * 4 + 3];

                    rgba[dstIndex + 0] = r;
                    rgba[dstIndex + 1] = g;
                    rgba[dstIndex + 2] = b;
                    rgba[dstIndex + 3] = a;
                    dstIndex += 4;
                }
            }

            GL.GenTextures(1, out uint tex);
            GL.BindTexture(GL.GL_TEXTURE_2D, tex);
            GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
            GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);
            GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR_MIPMAP_LINEAR);
            GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);

            fixed (byte* p = rgba)
            {
                GL.TexImage2D(
                    GL.GL_TEXTURE_2D,
                    0,
                    GL.GL_RGBA8,
                    bmp.Width,
                    bmp.Height,
                    0,
                    GL.GL_RGBA,
                    GL.GL_UNSIGNED_BYTE,
                    (IntPtr)p);
            }

            GL.GenerateMipmap(GL.GL_TEXTURE_2D);
            GL.BindTexture(GL.GL_TEXTURE_2D, 0);

            return tex;
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }
}