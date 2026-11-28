using System.Numerics;

namespace XuanYu.Editor.Camera;

public sealed partial class ViewportCamera
{
    private const double FieldOfView = Math.PI / 3d;
    private const double OrbitSpeed = 0.006d;
    private double _yaw, _pitch, _distance;
    private double _maxDistance = 500d;
    private (double X, double Y, double Z) _target;
    private double ZoomLimit => _terrainHeight is null ? _maxDistance : Math.Max(2000d, _maxDistance);
    public (double X, double Y, double Z) TargetMeters => _target;
    public double DistanceMeters => _distance;
    public (double X, double Y, double Z) PositionMeters
    {
        get
        {
            var b = Backward;
            return (_target.X + _distance * b.X, _target.Y + _distance * b.Y,
                _target.Z + _distance * b.Z);
        }
    }
    public Vector3 Target => ToVector(_target);
    public Vector3 Position => ToVector(PositionMeters);

    public ViewportCamera() => Reset();
    public ViewportCamera Copy() => (ViewportCamera)MemberwiseClone();

    public void Reset()
    {
        _target = default;
        _distance = Math.Sqrt(3.5d * 3.5d + 5.5d * 5.5d + 3.2d * 3.2d);
        _yaw = Math.Atan2(-5.5d, 3.5d);
        _pitch = Math.Asin(3.2d / _distance);
        LiftAboveTerrain();
    }

    public void Orbit(double deltaX, double deltaY)
    {
        double yaw = _yaw, pitch = _pitch;
        _yaw -= deltaX * OrbitSpeed;
        _pitch = Math.Clamp(_pitch + deltaY * OrbitSpeed, -Math.PI / 2d, Math.PI / 2d);
        if (!IsAboveTerrain()) { _yaw = yaw; _pitch = pitch; }
    }

    public void Pan(double deltaX, double deltaY, uint viewportHeight)
    {
        var oldTarget = _target;
        double units = 2d * _distance * Math.Tan(FieldOfView / 2d) / viewportHeight;
        var right = Right;
        var up = Up;
        _target = (_target.X + (-right.X * deltaX + up.X * deltaY) * units,
            _target.Y + (-right.Y * deltaX + up.Y * deltaY) * units,
            _target.Z + up.Z * deltaY * units);
        if (!IsAboveTerrain()) _target = oldTarget;
    }

    public void MoveGround(double forward, double right)
    {
        var oldTarget = _target;
        // 航向直接取 yaw，避免顶视或大坐标相减时丢失水平朝向。
        _target = (_target.X - Math.Cos(_yaw) * forward - Math.Sin(_yaw) * right,
            _target.Y - Math.Sin(_yaw) * forward + Math.Cos(_yaw) * right, _target.Z);
        if (!IsAboveTerrain()) _target = oldTarget;
    }

    public void Zoom(double wheelSteps)
    {
        double previous = _distance;
        _distance = Math.Clamp(_distance * Math.Pow(0.85d, wheelSteps), 0.25d, ZoomLimit);
        ConstrainZoom(previous);
    }

    public void Focus(Vector3 point) => Focus(point.X, point.Y, point.Z);

    public void Focus(double x, double y, double z)
    {
        _target = (x, y, z);
        LiftAboveTerrain();
    }

    public void Frame(Vector3 center, float radius, float aspect)
    {
        _target = (center.X, center.Y, center.Z);
        double fit = Math.Min(1d, aspect);
        double distance = Math.Max(0.5d, radius) * 1.35d / (Math.Sin(FieldOfView / 2d) * fit);
        _maxDistance = Math.Max(500d, distance * 4d);
        _distance = Math.Clamp(distance, 0.25d, ZoomLimit);
        LiftAboveTerrain();
    }
}