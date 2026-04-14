namespace Cat3d;

public struct Mat4
{
    public float[] M;

    public Mat4(bool identity)
    {
        M = new float[16];
        if (identity)
        {
            M[0] = 1.0f;
            M[5] = 1.0f;
            M[10] = 1.0f;
            M[15] = 1.0f;
        }
    }

    public static Mat4 Identity() => new Mat4(true);

    public static Mat4 Perspective(float fovYRadians, float aspect, float zNear, float zFar)
    {
        var r = new Mat4(false);
        float f = 1.0f / MathF.Tan(fovYRadians * 0.5f);

        r.M[0] = f / aspect;
        r.M[5] = f;
        r.M[10] = (zFar + zNear) / (zNear - zFar);
        r.M[11] = -1.0f;
        r.M[14] = (2.0f * zFar * zNear) / (zNear - zFar);

        return r;
    }

    public static Mat4 Translation(float x, float y, float z)
    {
        var r = Identity();
        r.M[12] = x;
        r.M[13] = y;
        r.M[14] = z;
        return r;
    }

    public static Mat4 RotationX(float a)
    {
        var r = Identity();
        float c = MathF.Cos(a);
        float s = MathF.Sin(a);

        r.M[5] = c;
        r.M[6] = s;
        r.M[9] = -s;
        r.M[10] = c;

        return r;
    }

    public static Mat4 RotationY(float a)
    {
        var r = Identity();
        float c = MathF.Cos(a);
        float s = MathF.Sin(a);

        r.M[0] = c;
        r.M[2] = -s;
        r.M[8] = s;
        r.M[10] = c;

        return r;
    }

    public static Mat4 Multiply(Mat4 a, Mat4 b)
    {
        var r = new Mat4(false);

        for (int col = 0; col < 4; ++col)
        {
            for (int row = 0; row < 4; ++row)
            {
                r.M[col * 4 + row] =
                    a.M[0 * 4 + row] * b.M[col * 4 + 0] +
                    a.M[1 * 4 + row] * b.M[col * 4 + 1] +
                    a.M[2 * 4 + row] * b.M[col * 4 + 2] +
                    a.M[3 * 4 + row] * b.M[col * 4 + 3];
            }
        }

        return r;
    }
}