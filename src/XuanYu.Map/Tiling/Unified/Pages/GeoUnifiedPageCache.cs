namespace XuanYu.World.Maps;

public sealed class GeoUnifiedPageCache
{
    private readonly GeoUnifiedTileLoader loader;
    private readonly GeoUnifiedSamples source;
    internal GeoUnifiedTileLoader Loader => loader;
    internal IDisposable BeginVersionOperation() => loader.BeginVersionOperation(source.Identity.Item2);

    public GeoUnifiedPageCache(GeoUnifiedTileLoader loader, GeoUnifiedSamples source)
    {
        if (!ReferenceEquals(loader, source.Loader))
            throw new ArgumentException("页缓存与采样器必须共用加载器。", nameof(loader));
        this.loader = loader;
        this.source = source;
    }
    internal const long PageCharge = 33L * 33 * 64;
    private readonly object _gate = new();
    private readonly Dictionary<GeoUnifiedTile, Entry> _pages = [];

    public PageLease? TryAcquire(GeoUnifiedTile tile)
    {
        lock (_gate)
        {
            if (!_pages.TryGetValue(tile, out var entry)) return null;
            entry.Pins++;
            return new PageLease(this, entry);
        }
    }

    public PageLease Acquire(GeoUnifiedTile tile)
    {
        lock (_gate)
        {
            if (!_pages.TryGetValue(tile, out var entry))
            {
                if (!loader.TryReserveOwnedPage(PageCharge)) throw new InvalidOperationException("高程页预算不足。");
                try
                {
                    var page = GeoUnifiedHeightPage.Create(tile, source);
                    for (int y = 0; y < 33; y++)
                        for (int x = 0; x < 33; x++)
                            if (page.StatusAt(x, y) != GeoSampleStatus.Valid)
                                throw new InvalidOperationException("高程页数据未全部就绪或无效。");
                    entry = new Entry(page);
                    _pages.Add(tile, entry);
                }
                catch { loader.ReleaseOwnedPage(PageCharge); throw; }
            }
            entry.Pins++;
            return new PageLease(this, entry);
        }
    }

    public bool Evict(GeoUnifiedTile tile)
    {
        lock (_gate)
        {
            if (!_pages.TryGetValue(tile, out var entry) || entry.Pins != 0) return false;
            _pages.Remove(tile);
            loader.ReleaseOwnedPage(PageCharge);
            return true;
        }
    }

    internal sealed class Entry(GeoUnifiedHeightPage page)
    {
        public readonly GeoUnifiedHeightPage Page = page;
        public int Pins;
    }

    public sealed class PageLease : IDisposable
    {
        private GeoUnifiedPageCache? _owner;
        private readonly Entry _entry;
        internal PageLease(GeoUnifiedPageCache owner, Entry entry) { _owner = owner; _entry = entry; }
        public GeoUnifiedHeightPage Page => _owner is not null ? _entry.Page :
            throw new ObjectDisposedException(nameof(PageLease));
        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            if (owner is null) return;
            lock (owner._gate) _entry.Pins--;
        }
    }
}
