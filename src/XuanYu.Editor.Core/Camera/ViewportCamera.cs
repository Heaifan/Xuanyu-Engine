using System.Numerics;

namespace XuanYu.Editor.Camera;

public sealed class ViewportCamera
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

    public void Reset()
    {
        Vector3 start = new(3.5f, -5.5f, 3.2f);
        Target = Vector3.Zero;
        _distance = start.Length();
        _yaw = MathF.Atan2(start.Y, start.X);
        _pitch = MathF.Asin(start.Z / _distance);
    }

    public void Orbit(float deltaX, float deltaY)
    {
        _yaw -= deltaX * OrbitSpeed;
        _pitch = Math.Clamp(_pitch + deltaY * OrbitSpeed, -1.48f, 1.48f);
    }

    public void Pan(float deltaX, float deltaY, uint viewportHeight)
    {
        Vector3 forward = Vector3.Normalize(Target - Position);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitZ));
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, forward));
        float unitsPerPixel = 2f * _distance * MathF.Tan(FieldOfView / 2f)
            / viewportHeight;
        Target += (-right * deltaX + up * deltaY) * unitsPerPixel;
    }

    public void Zoom(float wheelSteps)
    {
        _distance = Math.Clamp(
            _distance * MathF.Pow(0.85f, wheelSteps), 0.25f, 500f);
    }

    public void Focus(Vector3 point) => Target = point;

    public void Frame(Vector3 center, float radius, float aspect)
    {
        Target = center;
        float fit = MathF.Min(1f, aspect);
        _distance = Math.Clamp(
            MathF.Max(0.5f, radius) * 1.35f /
            (MathF.Sin(FieldOfView / 2f) * fit), 0.25f, 500f);
    }

    public Matrix4x4 ViewProjection(uint width, uint height)
    {
        Matrix4x4 view = Matrix4x4.CreateLookAt(
            Position, Target, Vector3.UnitZ);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView, (float)width / height, 0.1f, 1000f);
        projection.M22 = -projection.M22;
        return view * projection;
    }
}