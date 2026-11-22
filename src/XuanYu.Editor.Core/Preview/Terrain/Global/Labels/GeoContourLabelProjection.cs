using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoContourLabelProjection
{
    public static bool TryCrossing(GeoContourLabelSample a, GeoContourLabelSample b,
        int elevation, Func<double, double, double?> sample, EnuFrame anchor,
        Matrix4x4 projection, uint width, uint height, out GeoContourLabel label)
    {
        label = default;
        if (a.Height is not { } first || b.Height is not { } last || first == last ||
            elevation <= Math.Min(first, last) || elevation >= Math.Max(first, last)) return false;
        double low = 0d, high = 1d;
        for (int i = 0; i < 14; i++)
        {
            double t = (low + high) * 0.5d;
            double lat = a.Latitude + (b.Latitude - a.Latitude) * t;
            double lon = a.Longitude + (b.Longitude - a.Longitude) * t;
            double? h = sample(lat, lon);
            if (h is null) return false;
            if ((h.Value - elevation) * (first - elevation) > 0d) low = t;
            else high = t;
        }
        double ratio = (low + high) * 0.5d;
        double latitude = a.Latitude + (b.Latitude - a.Latitude) * ratio;
        double longitude = a.Longitude + (b.Longitude - a.Longitude) * ratio;
        if (!TryTangent(latitude, longitude, sample, out Vector2 tangent)) return false;
        EnuPosition local = anchor.ToEnu(Wgs84.ToEcef(
            new GeoCoordinate(latitude, longitude, 0d)));
        var point = new GeoContourLabel(0f, 0f, elevation, 0f,
            (float)local.East, (float)local.North, (float)(local.Up + elevation),
            tangent.X, tangent.Y, (int)Math.Floor(latitude),
            (int)Math.Floor(longitude));
        return TryScreen(point, projection, width, height, out label);
    }

    private static bool TryTangent(double lat, double lon,
        Func<double, double, double?> sample, out Vector2 tangent)
    {
        tangent = default;
        double dy = 20d / 111_320d;
        double dx = dy / Math.Cos(lat * Math.PI / 180d);
        double? e = sample(lat, lon + dx), w = sample(lat, lon - dx);
        double? n = sample(lat + dy, lon), s = sample(lat - dy, lon);
        if (e is null || w is null || n is null || s is null) return false;
        Vector2 t = new((float)(s.Value - n.Value), (float)(e.Value - w.Value));
        if (t.LengthSquared() < 1e-10f) return false;
        tangent = Vector2.Normalize(t);
        return true;
    }

    public static bool TryScreen(GeoContourLabel source, Matrix4x4 projection,
        uint width, uint height, out GeoContourLabel point)
    {
        point = default;
        Vector3 center = new(source.East, source.North, source.Up);
        Vector3 tangent = new(source.TangentEast, source.TangentNorth, source.TangentUp);
        tangent *= 20f;
        if (!Project(center, projection, width, height, out Vector2 p) ||
            !Project(center - tangent, projection, width, height, out Vector2 a) ||
            !Project(center + tangent, projection, width, height, out Vector2 b)) return false;
        float angle = MathF.Atan2(b.Y - a.Y, b.X - a.X) * 180f / MathF.PI;
        if (angle > 90f) angle -= 180f;
        if (angle < -90f) angle += 180f;
        if (MathF.Abs(angle) > 80f || p.X < 45f || p.X > width - 45f ||
            p.Y < 26f || p.Y > height - 26f ||
            (p.X > width - 110f && p.Y < 250f)) return false;
        point = source with { X = p.X, Y = p.Y, AngleDegrees = angle };
        return true;
    }

    private static bool Project(Vector3 p, Matrix4x4 m, uint w, uint h,
        out Vector2 pixel)
    {
        Vector4 c = Vector4.Transform(new Vector4(p, 1f), m);
        pixel = default;
        if (c.W <= 0f || c.Z < 0f || c.Z > c.W) return false;
        pixel = new Vector2((c.X / c.W + 1f) * w * 0.5f,
            (c.Y / c.W + 1f) * h * 0.5f);
        return true;
    }
}