namespace XuanYu.World.Maps;

public static class GeoHeightTileBuilder
{
    public static GeoHeightTile Build(NasademElevationSource source,
        GeoTileId id, int cellsPerEdge)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (cellsPerEdge < 1)
            throw new ArgumentOutOfRangeException(nameof(cellsPerEdge));

        GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
        int side = checked(cellsPerEdge + 1);
        double?[] heights = new double?[checked(side * side)];
        double latitudeStep = (bounds.NorthLatitudeDegrees -
            bounds.SouthLatitudeDegrees) / cellsPerEdge;
        double longitudeStep = (bounds.EastLongitudeDegrees -
            bounds.WestLongitudeDegrees) / cellsPerEdge;

        for (int row = 0; row < side; row++)
        {
            double latitude = row == cellsPerEdge
                ? bounds.SouthLatitudeDegrees
                : bounds.NorthLatitudeDegrees - row * latitudeStep;
            for (int column = 0; column < side; column++)
            {
                double longitude = column == cellsPerEdge
                    ? bounds.EastLongitudeDegrees
                    : bounds.WestLongitudeDegrees + column * longitudeStep;
                heights[row * side + column] = source.Sample(latitude, longitude);
            }
        }
        return new GeoHeightTile(id, cellsPerEdge, heights);
    }
}