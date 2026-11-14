
using System.Numerics;

namespace XuanYu.Editor.Camera;

public sealed partial class ViewportCamera
{
    private const float FieldOfView = MathF.PI / 3f;
    private const float OrbitSpeed = 0.006f;
    private float _yaw;
    private float _pitch;
    private float _distance;

    public Vector3 Target { get; private set; }
    public Vector3 Position => Target + _distance * new Vector3(
        MathF.Cos(_pitch) * MathF.Cos(_yaw),
        MathF.Cos(_pitch) * MathF.Sin(_yaw),
        MathF.Sin(_pitch));

    public ViewportCamera() => Reset();
    public ViewportCamera Copy() => (ViewportCamera)MemberwiseClone();

    public void Reset()
    {
        Vector3 start = new(3.5f, -5.5f, 3.2f);
        Target = Vector3.Zero;
        _distance = start.Length();
        _yaw = MathF.Atan2(start.Y, start.X);
        _pitch = MathF.Asin(start.Z / _distance);
        LiftAboveTerrain();
    }

    public void Orbit(float deltaX, float deltaY)
    {
        float yaw = _yaw, pitch = _pitch;
        _yaw -= deltaX * OrbitSpeed;
        _pitch = Math.Clamp(_pitch + deltaY * OrbitSpeed,
            -MathF.PI / 2f, MathF.PI / 2f);
        if (!IsAboveTerrain()) { _yaw = yaw; _pitch = pitch; }
    }

    public void Pan(float deltaX, float deltaY, uint viewportHeight)
    {
        Vector3 oldTarget = Target;
        float unitsPerPixel = 2f * _distance * MathF.Tan(FieldOfView / 2f)
            / viewportHeight;
        Target += (-ViewRight * deltaX + ViewUp * deltaY) * unitsPerPixel;
        if (!IsAboveTerrain()) Target = oldTarget;
    }

    public void MoveGround(float forward, float right)
    {
        Vector3 oldTarget = Target;
        Vector3 heading = Target - Position;
        heading.Z = 0f;
        heading = Vector3.Normalize(heading);
        Vector3 strafe = Vector3.Normalize(Vector3.Cross(heading, Vector3.UnitZ));
        Target += heading * forward + strafe * right;
        if (!IsAboveTerrain()) Target = oldTarget;
    }

    public void Zoom(float wheelSteps)
    {
        float previous = _distance;
        _distance = Math.Clamp(
            _distance * MathF.Pow(0.85f, wheelSteps), 0.25f, 500f);
        ConstrainZoom(previous);
    }

    public void Focus(Vector3 point)
    {
        Target = point;
        LiftAboveTerrain();
    }

    public void Frame(Vector3 center, float radius, float aspect)
    {
        Target = center;
        float fit = MathF.Min(1f, aspect);
        _distance = Math.Clamp(
            MathF.Max(0.5f, radius) * 1.35f /
            (MathF.Sin(FieldOfView / 2f) * fit), 0.25f, 500f);
        LiftAboveTerrain();
    }

    public Matrix4x4 ViewProjection(uint width, uint height)
    {
        Matrix4x4 view = Matrix4x4.CreateLookAt(Position, Target, ViewUp);
        Matrix4x4 projection = IsOrthographic
            ? Matrix4x4.CreateOrthographic(
                2f * _distance * MathF.Tan(FieldOfView / 2f) * (float)width / height,
                2f * _distance * MathF.Tan(FieldOfView / 2f), 0.1f, 1000f)
            : Matrix4x4.CreatePerspectiveFieldOfView(
                FieldOfView, (float)width / height, 0.1f, 1000f);
        projection.M22 = -projection.M22;
        return view * projection;
    }
}
