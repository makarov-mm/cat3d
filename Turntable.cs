namespace Cat3d;

// How the model is shown: turned by Yaw about its up axis, then tilted by Pitch toward the
// viewer (radians), with the camera Distance bounding radii away from its center.
internal readonly record struct Turntable(float Yaw, float Pitch, float Distance)
{
    private const float MaxPitch = 1.5f;
    private const float MinDistance = 1.2f;
    private const float MaxDistance = 6f;

    public static Turntable Default { get; } = new(0, 0, 2.3f);
    public Turntable Rotate(float yaw, float pitch) => new(MathF.IEEERemainder(Yaw + yaw, MathF.Tau), Math.Clamp(Pitch + pitch, -MaxPitch, MaxPitch), Distance);
    public Turntable Zoom(float factor) => this with { Distance = Math.Clamp(Distance * factor, MinDistance, MaxDistance) };
}
