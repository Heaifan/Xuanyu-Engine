namespace XuanYu.World.Maps;

public sealed class GeoHeightTile
{
    private readonly double?[] _heights;

    public GeoTileId Id { get; }
    public GeoTileBounds Bounds { get; }
    public int CellsPerEdge { get; }
    public int SamplesPerSide => CellsPerEdge + 1;
    public int ValidSampleCount { get; }
    public MapVerticalDatum VerticalDatum => MapVerticalDatum.Egm96Orthometric;

    internal GeoHeightTile(GeoTileId id, int cellsPerEdge, double?[] heights)
    {
        Id = id;
        Bounds = GeographicTileScheme.Bounds(id);
        CellsPerEdge = cellsPerEdge;
        _heights = heights;
        ValidSampleCount = heights.Count(value => value.HasValue);
    }

    public double? HeightAt(int column, int rowFromNorth)
    {
        if ((uint)column >= (uint)SamplesPerSide)
            throw new ArgumentOutOfRangeException(nameof(column));
        if ((uint)rowFromNorth >= (uint)SamplesPerSide)
            throw new ArgumentOutOfRangeException(nameof(rowFromNorth));
        return _heights[rowFromNorth * SamplesPerSide + column];
    }
}