using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    private (short Min, short Max)? _viewportHeights;

    public Func<GeoUnifiedTile, bool> CreateViewportVisibility(Matrix4x4 projection, EnuPosition renderOrigin = default)
    {
        var memo = new Dictionary<GeoUnifiedTile, bool>();
        return tile =>
        {
            if (!memo.TryGetValue(tile, out bool visible))
                memo[tile] = visible = IsViewportVisible(tile, projection, renderOrigin);
            return visible;
        };
    }

    private bool IsViewportVisible(GeoUnifiedTile tile, Matrix4x4 projection, EnuPosition renderOrigin)
    {
        if (_viewportHeights is not { } heights) return true;
        if (tile.West + tile.Span > 1_296_000 || tile.South + tile.Span > 648_000) return true;
        var center = GeoUnifiedPosition.At(_anchor, tile.West + tile.Span / 2,
            tile.South + tile.Span / 2, 0);
        // WGS84 经纬方向每弧度长度均小于 6400 km；两方向半幅相加是保守距离上界。
        double radius = 6_400_000d * tile.Span * Math.PI / (180d * 3600d) + 2d;
        if (_source.Geoid is not null)
        {
            // 椭球法线的高度分量会投影到全部 ENU 轴；200m 是格网入口已验证的上界。
            double reach = radius + Math.Max(Math.Abs((double)heights.Min), Math.Abs((double)heights.Max)) + 200d;
            return GeoUnifiedFrustum.Intersects(new GeoUnifiedBounds(
                new(center.East - reach, center.North - reach, center.Up - reach),
                new(center.East + reach, center.North + reach, center.Up + reach)).RelativeTo(renderOrigin), projection);
        }
        var bounds = new GeoUnifiedBounds(
            new(center.East - radius, center.North - radius, center.Up - radius + heights.Min),
            new(center.East + radius, center.North + radius, center.Up + radius + heights.Max));
        return GeoUnifiedFrustum.Intersects(bounds.RelativeTo(renderOrigin), projection);
    }
}