using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly bool _g06DiagnosticEnabled =
        Environment.GetEnvironmentVariable("XYE_G06_DIAG") == "1";
    private int _g06DiagnosticFrame;

    private void ReportG06Frame()
    {
        if (!_g06DiagnosticEnabled || ++_g06DiagnosticFrame % 120 != 1) return;
        GeoCoordinate eye = Wgs84.ToGeo(Camera.PositionEcef);
        GeoCoordinate target = Camera.Target;
        var zero = new GeoCoordinate(eye.LatitudeDegrees, eye.LongitudeDegrees, 0d);
        EnuPosition at = _anchor.ToEnu(Wgs84.ToEcef(zero));
        double? displayHeight = null;
        int hitTiles = 0;
        foreach (var (id, mesh) in _visibleMeshes)
        {
            GeoTileBounds b = GeographicTileScheme.Bounds(id);
            if (eye.LatitudeDegrees < b.SouthLatitudeDegrees ||
                eye.LatitudeDegrees > b.NorthLatitudeDegrees ||
                eye.LongitudeDegrees < b.WestLongitudeDegrees ||
                eye.LongitudeDegrees > b.EastLongitudeDegrees) continue;
            double? z = GeoTerrainMeshSurface.HeightAt(mesh, at.East, at.North);
            if (z is not { } height) continue;
            hitTiles++;
            GeoCoordinate terrain = Wgs84.ToGeo(_anchor.ToEcef(
                new EnuPosition(at.East, at.North, height)));
            displayHeight = displayHeight is { } old
                ? Math.Max(old, terrain.HeightMeters) : terrain.HeightMeters;
        }
        double? clearance = displayHeight is { } ground
            ? eye.HeightMeters - ground : null;
        bool inCoverage = _regions.Any(r =>
            eye.LatitudeDegrees >= r.SouthLatitudeDegrees &&
            eye.LatitudeDegrees <= r.NorthLatitudeDegrees &&
            eye.LongitudeDegrees >= r.WestLongitudeDegrees &&
            eye.LongitudeDegrees <= r.EastLongitudeDegrees);
        float near = NearPlaneMeters;
        string groundText = displayHeight?.ToString("F2") ?? "none";
        string gapText = clearance?.ToString("F2") ?? "none";
        Notice?.Invoke($"[G06/相机] 帧={_g06DiagnosticFrame} " +
            $"距离={Camera.DistanceMeters:F1}m 眼点=({eye.LatitudeDegrees:F6}," +
            $"{eye.LongitudeDegrees:F6},{eye.HeightMeters:F2}) " +
            $"目标=({target.LatitudeDegrees:F6},{target.LongitudeDegrees:F6}," +
            $"{target.HeightMeters:F2}) 俯仰={Camera.PitchRadians:F4} " +
            $"显示地高={groundText}m 净空={gapText}m Near={near:F2}m " +
            $"在数据范围={inCoverage} 地表命中片={hitTiles} " +
            $"可见片={_visibleMeshes.Count} 待细化={PendingCount} " +
            $"原始加载数={_source.Residency.LoadCount}");
    }
}