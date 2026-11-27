using System.Numerics;

namespace XuanYu.World.Maps;

public readonly record struct GeoUnifiedSurfaceHit(Vector3 Position, Vector3 A, Vector3 B,
    Vector3 C, Vector3 Heights, float Elevation)
{
    public bool TryContour(int interval, out int elevation, out Vector3 point, out Vector3 tangent)
    {
        if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));
        elevation = 0; point = tangent = default;
        float min = MathF.Min(Heights.X, MathF.Min(Heights.Y, Heights.Z));
        float max = MathF.Max(Heights.X, MathF.Max(Heights.Y, Heights.Z));
        int first = (int)MathF.Floor(min / interval) + 1;
        int last = (int)MathF.Ceiling(max / interval) - 1;
        if (first > last) return false;
        elevation = Math.Clamp((int)MathF.Round(Elevation / interval), first, last) * interval;
        Span<Vector3> points = stackalloc Vector3[3];
        int count = 0;
        if (Cross(A, B, Heights.X, Heights.Y, elevation, out var p)) points[count++] = p;
        if (Cross(B, C, Heights.Y, Heights.Z, elevation, out p)) points[count++] = p;
        if (Cross(C, A, Heights.Z, Heights.X, elevation, out p)) points[count++] = p;
        if (count < 2) return false;
        Vector3 a = points[0], b = points[1];
        if (a == b && count == 3) b = points[2];
        Vector3 delta = b - a;
        if (delta.LengthSquared() < 1e-8f) return false;
        tangent = Vector3.Normalize(delta);
        point = a + delta * Math.Clamp(Vector3.Dot(Position - a, delta) / delta.LengthSquared(), 0f, 1f);
        return true;
    }

    private static bool Cross(Vector3 a, Vector3 b, float ah, float bh, int elevation, out Vector3 p)
    {
        p = default;
        if (ah == bh || elevation < MathF.Min(ah, bh) || elevation > MathF.Max(ah, bh)) return false;
        p = Vector3.Lerp(a, b, (elevation - ah) / (bh - ah));
        return true;
    }
}