namespace XuanYu.World.Maps;

public static class NasademOverviewBuilder
{
    public static NasademOverviewGrid Build(NasademTile tile, NasademOverviewLevel level)
    {
        ArgumentNullException.ThrowIfNull(tile);
        int side = NasademOverviewLayout.Side(level);
        int stride = (int)level;
        var samples = new short[side * side];
        for (int row = 0; row < side; row++)
            for (int col = 0; col < side; col++)
                samples[row * side + col] =
                    tile.HeightAt(col * stride, row * stride) ?? short.MinValue;
        var info = new NasademHgtInfo(tile.TileId, tile.SouthLatitude, tile.WestLongitude);
        return new NasademOverviewGrid(info, level, side, samples);
    }
}