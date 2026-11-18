namespace XuanYu.World.Maps;

public sealed class GeoTerrainViewSession
{
    private readonly GeoTileSelector _selector;
    private readonly GeoTerrainTileSession _tiles;

    public int PendingCount => _tiles.PendingCount;

    public GeoTerrainViewSession(GeoTileSelector selector,
        GeoTerrainTileSession tiles)
    {
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        _tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
    }

    public GeoTerrainFrame Update(GeoTileView view, int maxBuilds,
        GeoTileBounds? coverage = null)
    {
        if (maxBuilds < 1)
            throw new ArgumentOutOfRangeException(nameof(maxBuilds));

        IReadOnlyList<GeoTileId> selected = _selector.Select(view, coverage);
        foreach (GeoTileId id in selected)
            _tiles.Request(id);

        int built = selected.Count == 0 ? 0 : _tiles.ProcessPending(maxBuilds);
        GeoTerrainFrame.Tile[] results = selected.Select(id =>
            new GeoTerrainFrame.Tile(id, _tiles.Request(id))).ToArray();
        return new GeoTerrainFrame(results, built, _tiles.PendingCount);
    }

    public GeoHeightTile? RequestExact(GeoTileId id)
    {
        _tiles.Request(id);
        return _tiles.TryGetExact(id);
    }
}