using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Preview.Terrain;

public static class GeoContourRibbon
{
    public static void Append(List<float> output, IReadOnlyList<GeoContourCoordinate> path,
        int levelIndex, EnuFrame anchor, GeoContourStyle style)
    {
        bool closed = path.Count > 2 && Near(path[0], path[^1]);
        int count = closed ? path.Count - 1 : path.Count;
        if (count < 2) return;
        Vector3[] p = new Vector3[count];
        float level = levelIndex * style.IntervalMeters;
        for (int i = 0; i < count; i++)
        {
            var geo = path[i];
            EnuPosition enu = anchor.ToEnu(Wgs84.ToEcef(
                new GeoCoordinate(geo.Latitude, geo.Longitude, 0d)));
            p[i] = new((float)enu.East, (float)enu.North,
                (float)(enu.Up + level) + style.LiftMeters);
        }
        bool major = levelIndex % 5 == 0;
        float radius = style.WidthMeters * (major ? 0.875f : 0.5f);
        Vector2[] offsets = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            Vector2 incoming = Direction(p[(i - 1 + count) % count], p[i]);
            Vector2 outgoing = Direction(p[i], p[(i + 1) % count]);
            if (!closed && i == 0) incoming = outgoing;
            if (!closed && i == count - 1) outgoing = incoming;
            Vector2 a = new(-incoming.Y, incoming.X);
            Vector2 b = new(-outgoing.Y, outgoing.X);
            Vector2 miter = a + b;
            if (miter.LengthSquared() < 1e-8f) miter = b;
            miter = Vector2.Normalize(miter);
            float dot = Vector2.Dot(miter, b);
            offsets[i] = miter * (radius / MathF.Max(0.5f, dot));
        }
        Vector3 ink = new(0.12f, 0.16f, 0.14f);
        Vector3 baseColor = TerrainColorPalette.Elevation(
            Math.Clamp((level + 200f) / 4200f, 0f, 1f));
        Vector3 color = Vector3.Lerp(baseColor, ink,
            MathF.Min(0.8f, style.Opacity * (major ? 1.8f : 1f)));
        int edges = closed ? count : count - 1;
        for (int i = 0; i < edges; i++)
        {
            int j = (i + 1) % count;
            Vector3 a = p[i], b = p[j];
            Vector3 oa = new(offsets[i], 0f), ob = new(offsets[j], 0f);
            Write(output, a - oa, color); Write(output, b - ob, color);
            Write(output, b + ob, color); Write(output, a - oa, color);
            Write(output, b + ob, color); Write(output, a + oa, color);
        }
    }

    private static bool Near(GeoContourCoordinate a, GeoContourCoordinate b) =>
        Math.Abs(a.Latitude - b.Latitude) < 1e-9d &&
        Math.Abs(a.Longitude - b.Longitude) < 1e-9d;

    private static Vector2 Direction(Vector3 a, Vector3 b)
    {
        Vector2 diff = new(b.X - a.X, b.Y - a.Y);
        return diff.LengthSquared() < 1e-12f ? Vector2.UnitX : Vector2.Normalize(diff);
    }

    private static void Write(List<float> output, Vector3 p, Vector3 color)
    {
        output.Add(p.X); output.Add(p.Y); output.Add(p.Z);
        output.Add(color.X); output.Add(color.Y); output.Add(color.Z);
        output.Add(1f);
    }
}