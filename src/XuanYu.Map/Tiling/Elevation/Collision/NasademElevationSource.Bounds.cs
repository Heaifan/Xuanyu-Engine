namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    // Bilinear patches attain their extrema at rectangle corners. Include both
    // clipped ends and each intervening grid line; missing samples use sea level.
    public double BoundRawHeight(NasademHgtInfo info, double west, double east,
        double south, double north)
    {
        NasademTile? tile = GetTile(info.SouthLatitude, info.WestLongitude);
        if (tile is null) return 0d;
        const int cells = NasademTile.Side - 1;
        double x0 = Math.Clamp((west - info.WestLongitude) * cells, 0d, cells);
        double x1 = Math.Clamp((east - info.WestLongitude) * cells, 0d, cells);
        double y0 = Math.Clamp((south - info.SouthLatitude) * cells, 0d, cells);
        double y1 = Math.Clamp((north - info.SouthLatitude) * cells, 0d, cells);
        if (x1 - x0 > 4d || y1 - y0 > 4d)
            return Math.Max(0d, tile.MaxElevation);
        double highest = double.NegativeInfinity;
        for (int row = (int)Math.Floor(y0); row <= Math.Ceiling(y1); row++)
            for (int col = (int)Math.Floor(x0); col <= Math.Ceiling(x1); col++)
            {
                double x = Math.Clamp(col, x0, x1), y = Math.Clamp(row, y0, y1);
                int i = Math.Min((int)x, cells - 1), j = Math.Min((int)y, cells - 1);
                double a = tile.HeightAt(i, j) ?? 0, b = tile.HeightAt(i + 1, j) ?? 0;
                double c = tile.HeightAt(i, j + 1) ?? 0, d = tile.HeightAt(i + 1, j + 1) ?? 0;
                double bottom = a + (b - a) * (x - i), top = c + (d - c) * (x - i);
                highest = Math.Max(highest, bottom + (top - bottom) * (y - j));
            }
        return tile.NoDataCount > 0 ? Math.Max(0d, highest) : highest;
    }
}
