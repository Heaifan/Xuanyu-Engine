namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    private sealed class Resource : IDisposable
    {
        private readonly GeoUnifiedPageCache.PageLease _lease;
        private readonly GeoUnifiedPageCache _cache;
        private readonly GeoUnifiedTileLoader _loader;
        private readonly GeoUnifiedTile _tile;
        private Func<Task<GeoUnifiedLodMetric>>? _prepare;
        private Task<GeoUnifiedLodMetric>? _metric;
        public GeoUnifiedBounds Bounds { get; }
        public string? Failure => _metric?.Exception?.GetBaseException().Message;

        public Resource(GeoUnifiedPageCache.PageLease lease, GeoUnifiedBounds bounds,
            Func<Task<GeoUnifiedLodMetric>> prepare, GeoUnifiedPageCache cache, GeoUnifiedTileLoader loader)
        {
            _lease = lease; _cache = cache; _loader = loader;
            _tile = lease.Page.Tile; Bounds = bounds; _prepare = prepare;
        }

        public Resource(GeoUnifiedPageCache.PageLease lease, GeoUnifiedLodMetric metric,
            GeoUnifiedPageCache cache, GeoUnifiedTileLoader loader)
            : this(lease, metric.Bounds, () => Task.FromResult(metric), cache, loader)
        {
            _metric = Task.FromResult(metric); _prepare = null;
        }

        public bool TryMetric(out GeoUnifiedLodMetric metric)
        {
            if (_prepare is { } prepare)
            {
                _prepare = null;
                _metric = prepare();
            }
            if (_metric is { IsCompletedSuccessfully: true })
            {
                metric = _metric.Result; return true;
            }
            metric = default; return false;
        }

        public void Dispose()
        {
            // 后台指标仍引用页租约，必须等测量结束才能释放预算和页。
            if (_metric is { IsCompleted: false } pending)
                _ = pending.ContinueWith(ReleaseAfterMetric, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            else ReleaseAfterMetric(_metric);
        }

        private void ReleaseAfterMetric(Task<GeoUnifiedLodMetric>? completed)
        {
            _ = completed?.Exception;
            _lease.Dispose();
            _loader.ReleaseOwnedPage(MetricCharge);
            _cache.Evict(_tile);
        }
    }
}