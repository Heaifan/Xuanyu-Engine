namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedPageLoader
{
    private readonly NasademTileCatalog catalog;
    private readonly GeoUnifiedTileLoader loader;
    private readonly GeoUnifiedPageCache cache;
    private readonly int maxPending;
    private readonly bool retainCatalog;
    internal GeoUnifiedTileLoader Loader => loader;
    internal GeoUnifiedPageCache Cache => cache;

    public GeoUnifiedPageLoader(NasademTileCatalog catalog, GeoUnifiedTileLoader loader,
        GeoUnifiedPageCache cache, int maxPending = 64, bool retainCatalog = false)
    {
        if (!ReferenceEquals(catalog, loader.Catalog) || !ReferenceEquals(cache.Loader, loader))
            throw new ArgumentException("页面请求、缓存及原始数据必须共用加载器和目录。");
        if (maxPending < 1) throw new ArgumentOutOfRangeException(nameof(maxPending));
        this.catalog = catalog; this.loader = loader; this.cache = cache;
        this.maxPending = maxPending; this.retainCatalog = retainCatalog;
    }
    private readonly object _gate = new();
    private readonly SemaphoreSlim _worker = new(1, 1);
    private readonly Dictionary<GeoUnifiedTile, Pending> _requests = [];

    public async Task<GeoUnifiedPageCache.PageLease> RequestAsync(GeoUnifiedTile tile)
    {
        using var versionOperation = cache.BeginVersionOperation();
        if (cache.TryAcquire(tile) is { } resident) return resident;
        Pending pending;
        lock (_gate)
        {
            if (!_requests.TryGetValue(tile, out pending!))
            {
                if (maxPending < 1 || _requests.Count >= maxPending)
                    throw new InvalidOperationException("高程页请求队列已满。");
                pending = new Pending();
                _requests.Add(tile, pending);
                pending.Work = PrepareAsync(tile);
            }
            pending.Waiters++;
        }
        try
        {
            await pending.Work.ConfigureAwait(false);
            return cache.TryAcquire(tile) ?? throw new InvalidOperationException("已就绪的高程页丢失。");
        }
        finally
        {
            lock (_gate)
            {
                if (--pending.Waiters == 0)
                {
                    _requests.Remove(tile);
                    if (pending.Work.IsCompletedSuccessfully) pending.Work.Result.Dispose();
                }
            }
        }
    }

    private async Task<GeoUnifiedPageCache.PageLease> PrepareAsync(GeoUnifiedTile tile)
    {
        await _worker.WaitAsync().ConfigureAwait(false);
        try
        {
            var keys = KeysFor(tile);
            if (!loader.CanFitPage(keys.Length, GeoUnifiedPageCache.PageCharge))
                throw new InvalidOperationException("高程页及其原始数据超出预算。");
            if (!loader.TrimUnneeded(keys))
                throw new InvalidOperationException("原始高程仍在使用，暂不能回收。");
            var sources = keys.Select(key => loader.Request(key.South, key.West)).ToArray();
            var loaded = await Task.WhenAll(sources).ConfigureAwait(false);
            loader.MarkEvictable(keys.Where((_, i) => loaded[i]));
            if (loaded.Any(ok => !ok)) throw new InvalidOperationException("原始高程未全部就绪。");
            return await Task.Run(() => cache.Acquire(tile)).ConfigureAwait(false);
        }
        finally { _worker.Release(); }
    }

    private (int South, int West)[] KeysFor(GeoUnifiedTile tile) => catalog.Coordinates
        .Where(key => retainCatalog || Intersects(tile, key.South, key.West)).ToArray();

    private static bool Intersects(GeoUnifiedTile tile, int south, int west)
    {
        int x = (west + 180) * 3600, y = (south + 90) * 3600;
        return tile.West <= x + 3600 && tile.West + tile.Span >= x &&
            tile.South <= y + 3600 && tile.South + tile.Span >= y;
    }

    private sealed class Pending
    {
        public Task<GeoUnifiedPageCache.PageLease> Work = null!;
        public int Waiters;
    }
}