namespace XuanYu.World.Maps;

public sealed record GeoTerrainFrame(
    IReadOnlyList<GeoTerrainFrame.Tile> Tiles,
    int BuiltCount,
    int PendingCount)
{
    public readonly record struct Tile(GeoTileId RequestedId,
        GeoHeightTile? AvailableTile)
    {
        public bool HasData => AvailableTile is { ValidSampleCount: > 0 };
        public bool IsExact => HasData && AvailableTile!.Id == RequestedId;
        public bool IsFallback => HasData && AvailableTile!.Id != RequestedId;
        public bool HasMissingSamples =>
            AvailableTile is { ValidSampleCount: > 0 } tile &&
            tile.ValidSampleCount < tile.SamplesPerSide * tile.SamplesPerSide;
    }
}