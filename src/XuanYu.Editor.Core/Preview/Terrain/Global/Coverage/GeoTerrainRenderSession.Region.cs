using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoTileBounds[] _regions;

    private static GeoTileBounds CreateCoverage(NasademHgtInfo[] tiles) => new(
        tiles.Min(t => t.SouthLatitude), tiles.Min(t => t.WestLongitude),
        tiles.Max(t => t.SouthLatitude) + 1d, tiles.Max(t => t.WestLongitude) + 1d);

    private static GeoTileBounds[] CreateRegions(NasademHgtInfo[] tiles, GeoTileBounds area)
    {
        double size = (area.NorthLatitudeDegrees - area.SouthLatitudeDegrees) *
            (area.EastLongitudeDegrees - area.WestLongitudeDegrees);
        // 完整矩形保留 G-04/G-05B 的原有网格；非矩形按实际 HGT 裁剪。
        if (size == tiles.Length) return [area];
        return tiles.Select(t => new GeoTileBounds(t.SouthLatitude, t.WestLongitude,
            t.SouthLatitude + 1d, t.WestLongitude + 1d)).ToArray();
    }

    private bool InCoverage(GeoTileId id)
    {
        GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
        return _regions.Any(region => Overlaps(bounds, region));
    }

    private static bool Overlaps(GeoTileBounds a, GeoTileBounds b) =>
        a.SouthLatitudeDegrees < b.NorthLatitudeDegrees &&
        a.NorthLatitudeDegrees > b.SouthLatitudeDegrees &&
        a.WestLongitudeDegrees < b.EastLongitudeDegrees &&
        a.EastLongitudeDegrees > b.WestLongitudeDegrees;
}