namespace XuanYu.World.Maps;

public static partial class GeographicTileScheme
{
    public static IEnumerable<GeoTileId> Cover(GeoTileBounds area, int level)
    {
        int columns = TileCountX(level);
        int rows = 1 << level;
        ValidateArea(area);

        int minX = Math.Clamp((int)Math.Floor(
            (area.WestLongitudeDegrees + 180d) * columns / 360d), 0, columns - 1);
        int maxX = Math.Clamp((int)Math.Ceiling(
            (area.EastLongitudeDegrees + 180d) * columns / 360d) - 1, 0, columns - 1);
        int minY = Math.Clamp((int)Math.Floor(
            (90d - area.NorthLatitudeDegrees) * rows / 180d), 0, rows - 1);
        int maxY = Math.Clamp((int)Math.Ceiling(
            (90d - area.SouthLatitudeDegrees) * rows / 180d) - 1, 0, rows - 1);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                yield return new GeoTileId(level, x, y);
    }

    private static void ValidateArea(GeoTileBounds area)
    {
        if (!double.IsFinite(area.SouthLatitudeDegrees) ||
            !double.IsFinite(area.NorthLatitudeDegrees) ||
            !double.IsFinite(area.WestLongitudeDegrees) ||
            !double.IsFinite(area.EastLongitudeDegrees) ||
            area.SouthLatitudeDegrees < -90d || area.NorthLatitudeDegrees > 90d ||
            area.SouthLatitudeDegrees >= area.NorthLatitudeDegrees ||
            area.WestLongitudeDegrees < -180d || area.EastLongitudeDegrees > 180d ||
            area.WestLongitudeDegrees >= area.EastLongitudeDegrees)
            throw new ArgumentOutOfRangeException(nameof(area));
    }
}