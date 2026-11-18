using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public static partial class GeographicTileScheme
{
    public static int TileCountX(int level)
    {
        if (level is < 0 or > GeoTileId.MaxLevel)
            throw new ArgumentOutOfRangeException(nameof(level));
        return 2 << level;
    }

    public static int TileCountY(int level)
    {
        if (level is < 0 or > GeoTileId.MaxLevel)
            throw new ArgumentOutOfRangeException(nameof(level));
        return 1 << level;
    }

    public static GeoTileBounds Bounds(GeoTileId tile)
    {
        double degrees = 180d / (1 << tile.Level);
        double west = -180d + tile.X * degrees;
        double north = 90d - tile.Y * degrees;
        return new GeoTileBounds(north - degrees, west, north, west + degrees);
    }

    public static GeoTileId Find(GeoCoordinate point, int level)
    {
        int columns = TileCountX(level);
        int rows = 1 << level;
        int x = Math.Min(columns - 1,
            (int)Math.Floor((point.LongitudeDegrees + 180d) * columns / 360d));
        int y = Math.Min(rows - 1,
            (int)Math.Floor((90d - point.LatitudeDegrees) * rows / 180d));
        return new GeoTileId(level, x, y);
    }
}