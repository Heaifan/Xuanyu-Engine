namespace XuanYu.World.Maps;

public sealed class GeoTerrainTileSession
{
    private readonly NasademElevationSource _source;
    private readonly GeoHeightTileCache _cache;
    private readonly GeoTileLoadQueue _requests = new();

    public int CellsPerEdge { get; }
    public int CachedCount => _cache.Count;
    public int PendingCount => _requests.Count;

    public GeoTerrainTileSession(NasademElevationSource source,
        int cellsPerEdge = 16, int cacheCapacity = 64)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (cellsPerEdge < 1)
            throw new ArgumentOutOfRangeException(nameof(cellsPerEdge));
        if (cacheCapacity < 1)
            throw new ArgumentOutOfRangeException(nameof(cacheCapacity));
        _source = source;
        _cache = new GeoHeightTileCache(cacheCapacity);
        CellsPerEdge = cellsPerEdge;
    }

    public GeoHeightTile? Request(GeoTileId id)
    {
        if (!_cache.TryGet(id, out GeoHeightTile? direct))
            _requests.Enqueue(id);
        if (direct is { ValidSampleCount: > 0 }) return direct;

        GeoTileId? ancestor = GeographicTileScheme.Parent(id);
        while (ancestor is GeoTileId parent)
        {
            if (_cache.TryGet(parent, out GeoHeightTile? tile) &&
                tile is not null &&
                tile.ValidSampleCount == tile.SamplesPerSide * tile.SamplesPerSide)
                return tile;
            ancestor = GeographicTileScheme.Parent(parent);
        }
        return null;
    }

    public GeoHeightTile? TryGetExact(GeoTileId id) =>
        _cache.TryGet(id, out GeoHeightTile? tile) ? tile : null;

    public int ProcessPending(int maxTiles)
    {
        if (maxTiles < 1)
            throw new ArgumentOutOfRangeException(nameof(maxTiles));
        int built = 0;
        while (built < maxTiles && _requests.Count > 0)
        {
            GeoTileId id = _requests.Dequeue();
            if (_cache.TryGet(id, out _)) continue;
            _cache.Add(GeoHeightTileBuilder.Build(_source, id, CellsPerEdge));
            built++;
        }
        return built;
    }
}