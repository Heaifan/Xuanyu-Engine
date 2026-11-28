using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.Preview;

public sealed class GeoGizmoDrag
{
    private readonly Matrix4x4 _matrix;
    private readonly uint _width, _height;
    private readonly Vector3 _normal, _start;
    public GeoGizmoHandle Handle { get; }
    private GeoGizmoDrag(Matrix4x4 matrix, uint width, uint height,
        GeoGizmoHandle handle, Vector3 normal, Vector3 start)
    { _matrix = matrix; _width = width; _height = height; Handle = handle; _normal = normal; _start = start; }

    public static GeoGizmoDrag? Begin(Matrix4x4 matrix, uint width, uint height,
        GeoGizmoHandle handle, Vector2 pixel)
    {
        if (handle == GeoGizmoHandle.None || !ViewportScreenRay.TryCreate(matrix, pixel, width, height,
            out var origin, out var ray)) return null;
        var axis = handle switch { GeoGizmoHandle.E => Vector3.UnitX,
            GeoGizmoHandle.N => Vector3.UnitY, GeoGizmoHandle.U => Vector3.UnitZ, _ => Vector3.Zero };
        var normal = handle switch { GeoGizmoHandle.EN => Vector3.UnitZ,
            GeoGizmoHandle.EU => Vector3.UnitY, GeoGizmoHandle.NU => Vector3.UnitX,
            _ => ray - Vector3.Dot(ray, axis) * axis };
        if (normal.LengthSquared() < .0025f) return null;
        normal = Vector3.Normalize(normal);
        return Intersect(origin, ray, normal, out var start) ?
            new(matrix, width, height, handle, normal, start) : null;
    }

    public bool TryDelta(Vector2 pixel, out EnuPosition delta)
    {
        delta = default;
        if (!ViewportScreenRay.TryCreate(_matrix, pixel, _width, _height, out var origin, out var ray) ||
            !Intersect(origin, ray, _normal, out var hit)) return false;
        var movement = hit - _start;
        delta = new(Handle.HasFlag(GeoGizmoHandle.E) ? movement.X : 0,
            Handle.HasFlag(GeoGizmoHandle.N) ? movement.Y : 0,
            Handle.HasFlag(GeoGizmoHandle.U) ? movement.Z : 0);
        return true;
    }

    private static bool Intersect(Vector3 origin, Vector3 ray, Vector3 normal, out Vector3 hit)
    {
        hit = default;
        float denominator = Vector3.Dot(ray, normal);
        if (MathF.Abs(denominator) < .05f) return false;
        float distance = -Vector3.Dot(origin, normal) / denominator;
        if (!float.IsFinite(distance) || distance < 0) return false;
        hit = origin + ray * distance;
        return float.IsFinite(hit.X) && float.IsFinite(hit.Y) && float.IsFinite(hit.Z);
    }
}