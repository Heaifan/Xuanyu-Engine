using XuanYu.World.Maps;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private GeoTileMonitorTile DescribeMonitorTile(GeoTileId id, bool targeted,
        bool drawn, bool cached, GeoTileId? fallback)
    {
        GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
        RenderMesh? mesh = _visibleMeshes.GetValueOrDefault(id);
        string asset = mesh?.AssetId ?? "";
        bool native = asset.StartsWith("terrain/g04/", StringComparison.Ordinal);
        bool rawCorrection = _rawGeometry.ContainsKey(id);
        string source = native || rawCorrection ? "原始HGT 1角秒" :
            id.Level <= 9 ? "Far 60角秒" : "Middle 12角秒";
        double coverageSpan = Math.Max(_coverage.NorthLatitudeDegrees - _coverage.SouthLatitudeDegrees,
            _coverage.EastLongitudeDegrees - _coverage.WestLongitudeDegrees);
        int minLevel = Math.Max(0, (int)Math.Ceiling(Math.Log2(180d / coverageSpan)));
        double step = native ? 1d / 3600d : 180d / Math.Pow(2, Math.Max(id.Level, minLevel) + 4);
        double latitude = (bounds.NorthLatitudeDegrees + bounds.SouthLatitudeDegrees) / 2d;
        var hgt = _collisionTiles.Where(tile => tile.SouthLatitude < bounds.NorthLatitudeDegrees &&
            tile.SouthLatitude + 1 > bounds.SouthLatitudeDegrees &&
            tile.WestLongitude < bounds.EastLongitudeDegrees &&
            tile.WestLongitude + 1 > bounds.WestLongitudeDegrees)
            .Select(tile => tile.TileId);
        return new GeoTileMonitorTile(id, bounds, targeted, drawn, cached, fallback)
        {
            MeshAssetId = asset,
            SourceHgt = string.Join("+", hgt),
            ElevationSource = mesh is null ? "未绘制" :
                rawCorrection && !native ? "原始高程点修正（网格未加密）" : source,
            GeometryNorthMeters = step * 111_132d,
            GeometryEastMeters = step * 111_320d * Math.Cos(latitude * Math.PI / 180d)
        };
    }
}
