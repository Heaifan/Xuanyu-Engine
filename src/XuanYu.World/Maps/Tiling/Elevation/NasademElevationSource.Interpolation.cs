namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    private static double? Interpolate(NasademTile tile, double latitude,
        double longitude)
    {
        const int cells = NasademTile.Side - 1;
        double x = (longitude - tile.WestLongitude) * cells;
        double y = (latitude - tile.SouthLatitude) * cells;
        x = Math.Clamp(SnapGrid(x), 0d, cells);
        y = Math.Clamp(SnapGrid(y), 0d, cells);
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        int x1 = Math.Min(x0 + 1, cells);
        int y1 = Math.Min(y0 + 1, cells);

        double? bottom = Mix(tile.HeightAt(x0, y0),
            tile.HeightAt(x1, y0), x - x0);
        double? top = Mix(tile.HeightAt(x0, y1),
            tile.HeightAt(x1, y1), x - x0);
        return Mix(bottom, top, y - y0);
    }

    private static double SnapGrid(double value)
    {
        double rounded = Math.Round(value);
        // 避免格点浮点尾差把 NoData 邻点错误纳入插值。
        return Math.Abs(value - rounded) < 1e-8 ? rounded : value;
    }

    private static double? Mix(double? a, double? b, double t)
    {
        if (t == 0d) return a;
        if (t == 1d) return b;
        return a is { } left && b is { } right
            ? left + (right - left) * t : null;
    }
}