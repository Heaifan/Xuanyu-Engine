namespace XuanYu.World.Maps;

public static partial class GeographicTileScheme
{
    public static GeoTileId? Parent(GeoTileId tile) => tile.Level == 0
        ? null : new GeoTileId(tile.Level - 1, tile.X / 2, tile.Y / 2);

    public static GeoTileId[] Children(GeoTileId tile)
    {
        if (tile.Level == GeoTileId.MaxLevel)
            throw new InvalidOperationException("当前已达到瓦片索引最大层级。");
        int level = tile.Level + 1;
        int x = tile.X * 2;
        int y = tile.Y * 2;
        // 北西、北东、南西、南东。
        return [new(level, x, y), new(level, x + 1, y),
            new(level, x, y + 1), new(level, x + 1, y + 1)];
    }

    public static GeoTileId East(GeoTileId tile)
    {
        int columns = TileCountX(tile.Level);
        return new GeoTileId(tile.Level,
            tile.X == columns - 1 ? 0 : tile.X + 1, tile.Y);
    }

    public static GeoTileId West(GeoTileId tile)
    {
        int columns = TileCountX(tile.Level);
        return new GeoTileId(tile.Level,
            tile.X == 0 ? columns - 1 : tile.X - 1, tile.Y);
    }

    public static GeoTileId? North(GeoTileId tile) => tile.Y == 0
        ? null : new GeoTileId(tile.Level, tile.X, tile.Y - 1);

    public static GeoTileId? South(GeoTileId tile) =>
        tile.Y == TileCountY(tile.Level) - 1
            ? null : new GeoTileId(tile.Level, tile.X, tile.Y + 1);
}