using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal static partial class GeoTerrainGridMesh
{
    public static RenderMesh? Create(EnuFrame anchor,
        Func<double, double, double?> sample, GeoCoordinate target,
        double step, bool geographic, long cx, long cy, double? longitudeStep = null, int? halfOverride = null)
    {
        double latStep = step / 111_000d;
        double lonStep = longitudeStep ?? latStep /
            Math.Max(0.25d, Math.Cos(target.LatitudeDegrees * Math.PI / 180d));
        int half = halfOverride ?? (geographic ? 6 : 32);
        int detail = (int)Math.Clamp(Math.Ceiling(step /
            (geographic ? 25d : 250d)), geographic ? 6d : 1d,
            geographic ? 128d : 16d);
        if (geographic && longitudeStep.HasValue)
            detail = Math.Min(detail, Math.Max(1, 4608 / (half * half)));
        double lift = Math.Clamp(step * 0.0025d, 2d, 12d);
        var vertices = new List<float>(100_000);
        Vector3 baseColor = geographic ? new(0.20f, 0.71f, 0.77f) : new(0.52f);
        for (int axis = 0; axis < 2; axis++)
        for (int i = -half; i <= half; i++)
        for (int j = -half; j < half;)
        {
            int stride = geographic || Math.Abs(j) < 8 ? 1 : Math.Abs(j) < 24 ? 2 : 4;
            int next = Math.Min(half, j + stride);
            double u0 = axis == 0 ? cx + i : cx + j;
            double v0 = axis == 0 ? cy + j : cy + i;
            double u1 = axis == 0 ? u0 : u0 + next - j;
            double v1 = axis == 0 ? v0 + next - j : v0;
            float edge = Math.Max(Math.Abs(i), Math.Max(Math.Abs(j),
                Math.Abs(next))) / (float)half;
            float fade = geographic ? 0f : Math.Clamp((edge - 0.65f) / 0.35f, 0f, 1f);
            long line = longitudeStep.HasValue ? (axis == 0 ? cx : cy) + i : i;
            Vector3 tint = Vector3.Lerp(line % 5 == 0 ? baseColor : baseColor * 0.68f,
                new Vector3(0.64f, 0.78f, 0.91f), fade);
            AddDrapedLine(vertices, Point, u0, v0, u1, v1,
                (float)(step * 0.003d), tint, detail);
            j = next;
        }
        return vertices.Count == 0 ? null
            : new RenderMesh(geographic ? "editor/geo-grid" : "editor/enu-grid",
                vertices.ToArray());

        Vector3? Point(double u, double v)
        {
            GeoCoordinate geo;
            if (geographic)
            {
                double lat = v * latStep, lon = u * lonStep;
                if (lat is < -90d or > 90d || lon is < -180d or > 180d)
                    return null;
                geo = new GeoCoordinate(lat, lon, 0d);
            }
            else
                geo = Wgs84.ToGeo(anchor.ToEcef(
                    new EnuPosition(u * step, v * step, 0d)));
            double? height = sample(geo.LatitudeDegrees, geo.LongitudeDegrees);
            if (height is null) return null;
            EnuPosition local = anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(
                geo.LatitudeDegrees, geo.LongitudeDegrees, height.Value + lift)));
            return new Vector3((float)local.East, (float)local.North, (float)local.Up);
        }
    }
}