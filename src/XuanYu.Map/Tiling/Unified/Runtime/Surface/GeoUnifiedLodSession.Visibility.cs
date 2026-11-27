using System.Numerics;

namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    private (short Min, short Max)? _viewportHeights;

    public Func<GeoUnifiedTile, bool> CreateViewportVisibility(Matrix4x4 projection)
    {
        var memo = new Dictionary<GeoUnifiedTile, bool>();
        return tile =>
        {
            if (!memo.TryGetValue(tile, out bool visible))
                memo[tile] = visible = IsViewportVisible(tile, projection);
            return visible;
        };
    }

    private bool IsViewportVisible(GeoUnifiedTile tile, Matrix4x4 projection)
    {
        if (_viewportHeights is not { } heights) return true;
        if (tile.West + tile.Span > 1_296_000 || tile.South + tile.Span > 648_000) return true;
        var center = GeoUnifiedPosition.At(_anchor, tile.West + tile.Span / 2,
            tile.South + tile.Span / 2, 0);
        // WGS84 经纬方向每弧度长度均小于 6400 km；两方向半幅相加是保守距离上界。
        double radius = 6_400_000d * tile.Span * Math.PI / (180d * 3600d) + 2d;
        var bounds = new GeoUnifiedBounds(
            new(center.East - radius, center.North - radius, center.Up - radius + heights.Min),
            new(center.East + radius, center.North + radius, center.Up + radius + heights.Max));
        return GeoUnifiedFrustum.Intersects(bounds, projection);
    }
}