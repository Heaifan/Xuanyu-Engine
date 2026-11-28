using System.Numerics;

namespace XuanYu.Editor.Preview;

public sealed class GeoGizmoOrientationDrag
{
    private readonly GeoGizmoLayout _layout;
    private readonly Vector2 _start, _a, _b;
    private float _previous, _angle;
    public GeoGizmoHandle Handle { get; }
    public GeoGizmoMode Mode => _layout.Mode;

    public GeoGizmoOrientationDrag(GeoGizmoLayout layout, GeoGizmoHandle handle, Vector2 pixel)
    {
        _layout = layout; Handle = handle; _start = pixel;
        if (handle == GeoGizmoHandle.All) return;
        var points = layout.Handles.Single(h => h.Id == handle).Points;
        _a = points[Mode == GeoGizmoMode.Rotate ? 0 : 1] - layout.Center;
        if (Mode == GeoGizmoMode.Rotate)
        {
            _b = points[16] - layout.Center;
            _previous = Angle(pixel);
        }
    }

    public Vector3 Value(Vector2 pixel, bool fine, bool snap)
    {
        float value;
        if (Mode == GeoGizmoMode.Rotate)
        {
            if (Vector2.DistanceSquared(pixel, _layout.Center) > 16)
            {
                float next = Angle(pixel);
                _angle += MathF.IEEERemainder(next - _previous, MathF.Tau);
                _previous = next;
            }
            value = _angle * (180 / MathF.PI);
            value = fine ? value * .2f : snap ? MathF.Round(value / 5) * 5 : value;
        }
        else
        {
            var movement = pixel - _start;
            float delta = Handle == GeoGizmoHandle.All ? -movement.Y / 80 :
                Vector2.Dot(movement, _a) / _a.LengthSquared();
            value = 1 + (fine ? delta * .2f : delta);
            if (!fine && snap) value = MathF.Round(value * 10) / 10;
            value = Math.Clamp(value, .01f, 100f);
        }
        float neutral = Mode == GeoGizmoMode.Rotate ? 0 : 1;
        return new(Handle.HasFlag(GeoGizmoHandle.E) ? value : neutral,
            Handle.HasFlag(GeoGizmoHandle.N) ? value : neutral,
            Handle.HasFlag(GeoGizmoHandle.U) ? value : neutral);
    }

    private float Angle(Vector2 pixel)
    {
        var p = pixel - _layout.Center;
        float det = _a.X * _b.Y - _a.Y * _b.X;
        return MathF.Atan2((_a.X * p.Y - _a.Y * p.X) / det,
            (p.X * _b.Y - p.Y * _b.X) / det);
    }
}