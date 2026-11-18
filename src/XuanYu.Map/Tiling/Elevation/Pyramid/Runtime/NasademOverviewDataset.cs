namespace XuanYu.World.Maps;

internal sealed class NasademOverviewDataset
{
    private readonly Dictionary<(int South, int West),
        (NasademCatalogEntry Entry, NasademOverviewGrid Far)> _grids = [];
    private readonly NasademOverviewDiskCache? _disk;
    private readonly NasademMiddleCache _middle;
    private long _farBytes;
    public int Count => _grids.Count;
    public long SampleBytes => _farBytes + _middle.Bytes;
    public int CacheHits { get; private set; }
    public int MiddleCount => _middle.Count;
    public int MiddleCapacity => _middle.Capacity;

    public NasademOverviewDataset(NasademTileCatalog catalog, string? cacheDirectory = null,
        int middleCapacity = 64)
    {
        if (middleCapacity < 1) throw new ArgumentOutOfRangeException(nameof(middleCapacity));
        _disk = cacheDirectory is null ? null : new NasademOverviewDiskCache(cacheDirectory);
        _middle = new NasademMiddleCache(_disk is null ? Math.Max(1, catalog.Count) : middleCapacity);
        foreach (NasademCatalogEntry entry in catalog.Entries)
        {
            var pair = _disk is null ? Build(catalog, entry) : _disk.Load(entry);
            _grids.Add((entry.Info.SouthLatitude, entry.Info.WestLongitude), (entry, pair.Far));
            _farBytes += pair.Far.SampleBytes;
            _middle.Add(pair.Middle);
            if (pair.Hit) CacheHits++;
        }
    }

    private static (NasademOverviewGrid Far, NasademOverviewGrid Middle, bool Hit)
        Build(NasademTileCatalog catalog, NasademCatalogEntry entry)
    {
        NasademTile tile = catalog.Load(entry.Info.SouthLatitude, entry.Info.WestLongitude)!;
        return (NasademOverviewBuilder.Build(tile, NasademOverviewLevel.Far),
            NasademOverviewBuilder.Build(tile, NasademOverviewLevel.Middle), false);
    }

    public NasademOverviewGrid? Get(int south, int west, NasademOverviewLevel level)
    {
        if (!_grids.TryGetValue((south, west), out var item)) return null;
        if (level == NasademOverviewLevel.Far) return item.Far;
        var grid = _middle.Get(south, west);
        if (grid is not null) return grid;
        grid = _disk!.ReadMiddle(item.Entry);
        _middle.Add(grid);
        return grid;
    }
}