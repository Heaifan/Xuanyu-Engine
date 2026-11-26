namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedCoverage
{
    public int EdgeMask(GeoUnifiedTile tile)
    {
        if (!_leaves.Contains(tile)) throw new ArgumentException("瓦片不在已提交覆盖内。", nameof(tile));
        int mask = 0;
        foreach (var neighbor in _leaves)
        {
            if (neighbor.Level != tile.Level + 1 || !Adjacent(tile, neighbor)) continue;
            if (neighbor.South + neighbor.Span == tile.South) mask |= 1;
            if (tile.South + tile.Span == neighbor.South) mask |= 2;
            if (neighbor.West + neighbor.Span == tile.West) mask |= 4;
            if (tile.West + tile.Span == neighbor.West) mask |= 8;
        }
        return mask;
    }
}
