using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoContourMesh
{
    internal static void TraceTriangle(List<float> output, Point a, Point b,
        Point c, GeoContourStyle style)
    {
        float min = MathF.Min(a.Elevation, MathF.Min(b.Elevation, c.Elevation));
        float max = MathF.Max(a.Elevation, MathF.Max(b.Elevation, c.Elevation));
        int start = (int)MathF.Ceiling(min / style.IntervalMeters);
        int end = (int)MathF.Ceiling(max / style.IntervalMeters);
        Span<Point> cuts = stackalloc Point[3];
        for (int k = start; k < end; k++)
        {
            float level = k * style.IntervalMeters;
            int count = 0;
            if (Cross(a, b, level, out Point ab)) cuts[count++] = ab;
            if (Cross(b, c, level, out Point bc)) cuts[count++] = bc;
            if (Cross(c, a, level, out Point ca)) cuts[count++] = ca;
            if (count == 2) WriteRibbon(output, cuts[0], cuts[1], style, k % 5 == 0);
        }
    }

    private static bool Cross(Point a, Point b, float level, out Point cut)
    {
        cut = default;
        if ((a.Elevation <= level) == (b.Elevation <= level)) return false;
        float t = (level - a.Elevation) / (b.Elevation - a.Elevation);
        cut = new Point(Vector3.Lerp(a.Position, b.Position, t),
            Vector3.Lerp(a.Color, b.Color, t), level);
        return true;
    }

    private static void WriteRibbon(List<float> output, Point a, Point b,
        GeoContourStyle style, bool major)
    {
        Vector2 direction = new(b.Position.X - a.Position.X,
            b.Position.Y - a.Position.Y);
        if (direction.LengthSquared() < 0.000001f) return;
        float width = style.WidthMeters * (major ? 1.75f : 1f);
        direction = Vector2.Normalize(direction) * (width * 0.5f);
        Vector3 side = new(-direction.Y, direction.X, 0f);
        Vector3 lift = new(0f, 0f, style.LiftMeters);
        Vector3 a0 = a.Position - side + lift, a1 = a.Position + side + lift;
        Vector3 b0 = b.Position - side + lift, b1 = b.Position + side + lift;
        Vector3 ink = new(0.12f, 0.16f, 0.14f);
        float opacity = MathF.Min(0.8f, style.Opacity * (major ? 1.8f : 1f));
        Vector3 ca = Vector3.Lerp(a.Color, ink, opacity);
        Vector3 cb = Vector3.Lerp(b.Color, ink, opacity);
        Write(output, a0, ca); Write(output, b0, cb); Write(output, b1, cb);
        Write(output, a0, ca); Write(output, b1, cb); Write(output, a1, ca);
    }

    private static void Write(List<float> output, Vector3 p, Vector3 color)
    {
        output.Add(p.X); output.Add(p.Y); output.Add(p.Z);
        output.Add(color.X); output.Add(color.Y); output.Add(color.Z);
        output.Add(1f);
    }
}