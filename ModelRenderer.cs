using System.Numerics;

namespace Cat3d;

// Draws a mesh in front of the camera, lit by a directional light that swings from side to
// side (Blinn-Phong), with its diffuse map and bump mapping from its height map.
internal sealed class ModelRenderer : IDisposable
{
    private const float FieldOfView = MathF.PI / 3;   // vertical
    private const float BumpDepth = 0.2f;             // model units from black to white in the height map
    private const float LightSwingSeconds = 13f;      // across and back

    // The cat was modelled with Z up (3ds Max); the viewer's up is Y.
    private static readonly Matrix4x4 ZUpToYUp = Matrix4x4.CreateRotationX(-MathF.PI / 2);

    private readonly GpuMesh _mesh;
    private readonly Texture _diffuseMap;
    private readonly Texture? _bumpMap;
    private readonly ShaderProgram _program;
    private readonly int _modelUniform;
    private readonly int _viewProjectionUniform;
    private readonly int _cameraPositionUniform;
    private readonly int _lightDirectionUniform;
    private readonly Vector3 _center;
    private readonly float _radius;

    public ModelRenderer(ObjMesh mesh)
    {
        _center = mesh.Center;
        _radius = mesh.Radius;
        _mesh = new GpuMesh(mesh.Vertices, mesh.Indices);
        _diffuseMap = mesh.DiffuseMap is { } diffuseMap ? Texture.Load(diffuseMap) : Texture.Solid(Color.White);
        _bumpMap = mesh.BumpMap is { } bumpMap ? Texture.Load(bumpMap, grayscale: true) : null;

        _program = new ShaderProgram(ReadShader("cat.vert"), ReadShader("cat.frag"));
        _modelUniform = _program.Uniform("uModel");
        _viewProjectionUniform = _program.Uniform("uViewProjection");
        _cameraPositionUniform = _program.Uniform("uCameraPosition");
        _lightDirectionUniform = _program.Uniform("uLightDirection");

        _program.Use();
        GL.Uniform1(_program.Uniform("uDiffuseMap"), 0);
        GL.Uniform1(_program.Uniform("uBumpMap"), 1);
        GL.Uniform1(_program.Uniform("uBumpDepth"), _bumpMap is null ? 0f : BumpDepth);

        GL.Enable(GL.GL_DEPTH_TEST);
        GL.Enable(GL.GL_CULL_FACE);   // the back faces
        GL.ClearColor(0.8f, 0.8f, 0.8f, 1.0f);
    }

    public void Render(int width, int height, Turntable turntable, float seconds)
    {
        float distance = turntable.Distance * _radius;
        Matrix4x4 model = Matrix4x4.CreateTranslation(-_center) * ZUpToYUp
            * Matrix4x4.CreateRotationY(turntable.Yaw) * Matrix4x4.CreateRotationX(turntable.Pitch);
        Matrix4x4 view = Matrix4x4.CreateTranslation(0, 0, -distance);
        Matrix4x4 projection = Perspective(FieldOfView, (float)width / height, 0.05f * _radius, distance + 2 * _radius);
        // From the camera's side, up to 45° to the left and right.
        var lightDirection = new Vector3(2 * MathF.Sin(MathF.Tau * seconds / LightSwingSeconds), 0, -2);

        GL.Viewport(0, 0, width, height);
        GL.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

        _program.Use();
        GL.UniformMatrix4(_modelUniform, model);
        GL.UniformMatrix4(_viewProjectionUniform, view * projection);
        GL.Uniform3(_cameraPositionUniform, new Vector3(0, 0, distance));
        GL.Uniform3(_lightDirectionUniform, lightDirection);
        _diffuseMap.Bind(0);
        _bumpMap?.Bind(1);
        _mesh.Draw();
    }

    public void Dispose()
    {
        _program.Dispose();
        _bumpMap?.Dispose();
        _diffuseMap.Dispose();
        _mesh.Dispose();
    }

    // For OpenGL's clip space, where depth runs from -1 to 1; Matrix4x4.CreatePerspective*
    // are made for Direct3D's 0 to 1.
    private static Matrix4x4 Perspective(float fieldOfViewY, float aspect, float near, float far)
    {
        float f = 1.0f / MathF.Tan(fieldOfViewY * 0.5f);
        return new Matrix4x4(
            f / aspect, 0, 0, 0,
            0, f, 0, 0,
            0, 0, (far + near) / (near - far), -1,
            0, 0, 2.0f * far * near / (near - far), 0);
    }

    // The shaders are built into the executable.
    private static string ReadShader(string name)
    {
        using Stream stream = typeof(ModelRenderer).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"The shader {name} is missing from the executable.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
