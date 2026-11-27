namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedCoverage
{
    public int EdgeMask(GeoUnifiedTile tile)
    {
        if (!_leaves.Contains(tile)) throw new ArgumentException("瓦片不在已提交覆盖内。", nameof(tile));
        return CoarseNeighborMask(tile);
    }

    // 2:1 接缝只需要查询上一级的四个可能邻块；粗化父块也适用。
    internal int CoarseNeighborMask(GeoUnifiedTile tile)
    {
        if (tile.Level == 15) return 0;
        int x = tile.X / 2, y = tile.Y / 2, level = tile.Level + 1;
        int mask = 0;
        if (tile.Y % 2 == 0 && ContainsCoarse(level, x, y - 1)) mask |= 1;
        if (tile.Y % 2 == 1 && ContainsCoarse(level, x, y + 1)) mask |= 2;
        if (tile.X % 2 == 0 && ContainsCoarse(level, x - 1, y)) mask |= 4;
        if (tile.X % 2 == 1 && ContainsCoarse(level, x + 1, y)) mask |= 8;
        return mask;
    }

    private bool ContainsCoarse(int level, int x, int y)
    {
        int span = 32 << level;
        return x >= 0 && y >= 0 && (long)x * span < 1_296_000 &&
            (long)y * span < 648_000 && _leaves.Contains(new GeoUnifiedTile(level, x, y));
    }
}