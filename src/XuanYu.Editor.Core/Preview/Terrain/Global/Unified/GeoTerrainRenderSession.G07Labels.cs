using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public GeoContourLabel[] BuildG07Labels(GeoUnifiedLodSession cpu, Matrix4x4 projection,
        uint width, uint height, int interval, float scale,
        IReadOnlyList<GeoContourLabel>? previous = null, bool refill = true)
    {
        if (interval <= 0 || width < 200 || height < 150) return [];
        int major = Math.Max(100, interval * 5);
        var accepted = new List<GeoContourLabel>(8);
        foreach (var old in previous ?? [])
        {
            if (old.ElevationMeters % major != 0 ||
                !GeoContourLabelProjection.TryScreen(old, projection, width, height, out var projected) ||
                !TryG07Label(cpu, projection, width, height, major, new(projected.X, projected.Y), out var rebound) ||
                rebound.ElevationMeters != old.ElevationMeters ||
                Vector2.Distance(new(rebound.X, rebound.Y), new(projected.X, projected.Y)) > 12f * scale)
                continue;
            AcceptG07Label(cpu, projection, width, height, scale, accepted, rebound);
        }
        if (!refill || accepted.Count == 8) return accepted.ToArray();
        var candidates = new List<GeoContourLabel>(24);
        for (int row = 1; row <= 4; row++)
            for (int col = 1; col <= 6; col++)
                if (TryG07Label(cpu, projection, width, height, major,
                    new(width * col / 7f, height * row / 5f), out var candidate))
                    candidates.Add(candidate);
        foreach (var label in GeoContourLabelLayout.Select(candidates, width, height))
        {
            AcceptG07Label(cpu, projection, width, height, scale, accepted, label);
            if (accepted.Count == 8) break;
        }
        return accepted.ToArray();
    }

    private bool TryG07Label(GeoUnifiedLodSession cpu, Matrix4x4 projection,
        uint width, uint height, int major, Vector2 pixel, out GeoContourLabel label)
    {
        label = default;
        if (!ViewportScreenRay.TryCreate(projection, pixel, width, height, out var start, out var ray) ||
            !cpu.TryPickSurfaceHit(start, ray, out var hit) ||
            !hit.TryContour(major, out int elevation, out var point, out var tangent)) return false;
        var geo = Wgs84.ToGeo(_anchor.ToEcef(new EnuPosition(point.X, point.Y, point.Z)));
        if (cpu.Domain is { } domain && !domain.Contains(geo.LatitudeDegrees, geo.LongitudeDegrees) ||
            GeoContourLabelWater.NearWater(_waterAtlas, geo)) return false;
        var candidate = new GeoContourLabel(0, 0, elevation, 0, point.X, point.Y, point.Z,
            tangent.X, tangent.Y, (int)Math.Floor(geo.LatitudeDegrees), (int)Math.Floor(geo.LongitudeDegrees))
            { TangentUp = tangent.Z };
        return GeoContourLabelProjection.TryScreen(candidate, projection, width, height, out label);
    }

    private static void AcceptG07Label(GeoUnifiedLodSession cpu, Matrix4x4 projection,
        uint width, uint height, float scale, List<GeoContourLabel> accepted, GeoContourLabel label)
    {
        if (accepted.Any(old => G07LabelsConflict(old, label, scale))) return;
        if (!ViewportScreenRay.TryCreate(projection, new Vector2(label.X, label.Y), width, height,
            out var start, out var ray) || !cpu.TryPickSurfaceHit(start, ray, out var hit) ||
            Vector3.Distance(hit.Position, new(label.East, label.North, label.Up)) > 2f) return;
        accepted.Add(label);
    }

    internal static bool G07LabelsConflict(GeoContourLabel a, GeoContourLabel b, float scale)
    {
        float dx = MathF.Abs(a.X - b.X), dy = MathF.Abs(a.Y - b.Y);
        if (dx < 72f * scale && dy < 44f * scale) return true;
        // G07 尚无连通等高线 ID；同高程仅做局部屏幕去重。
        float radius = 450f * scale;
        return a.ElevationMeters == b.ElevationMeters && dx * dx + dy * dy < radius * radius;
    }
}