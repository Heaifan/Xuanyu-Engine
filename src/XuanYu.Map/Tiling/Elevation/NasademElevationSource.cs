namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    private readonly Dictionary<(int South, int West), NasademTile> _tiles = [];
    private readonly NasademTileCache? _cache;

    public NasademElevationSource(params NasademTile[] tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        foreach (NasademTile tile in tiles)
            _tiles.Add((tile.SouthLatitude, tile.WestLongitude), tile);
    }

    public NasademElevationSource(NasademTileCatalog catalog, int cacheCapacity = 8,
        bool useOverview = false, string? overviewCacheDirectory = null,
        int middleCapacity = 64)
    {
        _cache = new NasademTileCache(catalog, cacheCapacity);
        if (useOverview)
        {
            var dataset = new NasademOverviewDataset(catalog, overviewCacheDirectory, middleCapacity);
            _far = new NasademElevationSource(dataset, NasademOverviewLevel.Far);
            _middle = new NasademElevationSource(dataset, NasademOverviewLevel.Middle);
        }
    }

    public NasademSourceResidency Residency => _cache?.Stats ??
        new(false, _tiles.Count, _tiles.Count, 0, 0);

    public NasademTileSummary? ReadSummary(int south, int west) =>
        GetTile(south, west) is { } tile
            ? new NasademTileSummary(tile.MinElevation, tile.MaxElevation, tile.NoDataCount) : null;

    // 缓存未命中时返回 null，绝不为渲染纹理同步读取 HGT。
    public NasademTile? TryGetResidentTile(int south, int west) =>
        _cache is null ? _tiles.GetValueOrDefault((south, west))
            : _cache.FindResident(south, west);

    private NasademTile? GetTile(int south, int west) => _cache is not null
        ? _cache.Get(south, west) : _tiles.GetValueOrDefault((south, west));
}