using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private int ContourPrecision(GeoContourStyle style)
    {
        int minimum = style.IntervalMeters switch
        {
            <= 50 => 1,
            <= 100 => 2,
            <= 200 => 4,
            <= 500 => 5,
            _ => 10
        };
        double largest = 0d;
        foreach (GeoTileId id in _visibleMeshes.Keys)
        {
            GeoTileBounds b = GeographicTileScheme.Bounds(id);
            double width = Math.Min(b.EastLongitudeDegrees, _coverage.EastLongitudeDegrees) -
                Math.Max(b.WestLongitudeDegrees, _coverage.WestLongitudeDegrees);
            double height = Math.Min(b.NorthLatitudeDegrees, _coverage.NorthLatitudeDegrees) -
                Math.Max(b.SouthLatitudeDegrees, _coverage.SouthLatitudeDegrees);
            largest = Math.Max(largest, Math.Max(width, height));
        }
        int required = Math.Max(minimum, (int)Math.Ceiling(largest * 3600d / 128d));
        foreach (int step in new[] { 1, 2, 4, 5, 10, 20, 50, 100, 200, 500, 1000 })
            if (step >= required) return step;
        return 1000;
    }
}