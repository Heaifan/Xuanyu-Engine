namespace XuanYu.World.Maps;

public static class NasademPreviewBuilder
{
    public static MapDocument Create(NasademTile tile,
        TerrainWaterMask? water = null)
    {
        ArgumentNullException.ThrowIfNull(tile);
        var geo = new NasademGeoReference(tile);
        int size = NasademTile.Side;
        var field = new TerrainHeightField(size, size,
            (float)geo.SampleSpacingXMeters,
            (float)geo.SampleSpacingYMeters);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                field.SetHeight(x, y, tile.HeightAt(x, y) ?? 0f);

        var reference = new MapGeoReference(tile.SouthLatitude,
            tile.WestLongitude, tile.SouthLatitude + 1d,
            tile.WestLongitude + 1d, MapVerticalDatum.Egm96Orthometric);
        return new MapDocument($"NASADEM_{tile.TileId}", field, water, reference);
    }
}