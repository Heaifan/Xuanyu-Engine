namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    private const long MetricCharge = 512;

    private Task<Resource>? Start(GeoUnifiedTile tile, int mask)
    {
        var key = (tile, mask);
        if (_resources.TryGetValue(key, out var task)) return task;
        if (_resources.Count >= _resourceLimit)
        {
            Diagnostic = "地形指标资源达到上限。";
            return null;
        }
        task = PrepareAsync(tile, mask);
        _resources.Add(key, task);
        return task;
    }

    private async Task<Resource> PrepareAsync(GeoUnifiedTile tile, int mask)
    {
        var lease = await _pages.RequestAsync(tile).ConfigureAwait(false);
        bool charged = false;
        try
        {
            if (!_loader.TryReserveOwnedPage(MetricCharge))
                throw new InvalidOperationException("地形指标预算不足。");
            charged = true;
            await _metricWorker.WaitAsync().ConfigureAwait(false);
            try
            {
                using var pin = await _pages.PinReferenceAsync(tile).ConfigureAwait(false);
                var metric = await Task.Run(() => GeoUnifiedLodMetric.Measure(
                    lease.Page, _source, _anchor, mask)).ConfigureAwait(false);
                if (metric is null) throw new InvalidOperationException("参考高程未完整就绪。");
                return new Resource(lease, metric.Value, _cache, _loader);
            }
            finally { _metricWorker.Release(); }
        }
        catch
        {
            if (charged) _loader.ReleaseOwnedPage(MetricCharge);
            lease.Dispose();
            _cache.Evict(tile);
            throw;
        }
    }

    private sealed class Resource(
        GeoUnifiedPageCache.PageLease lease, GeoUnifiedLodMetric metric,
        GeoUnifiedPageCache cache, GeoUnifiedTileLoader loader) : IDisposable
    {
        public GeoUnifiedLodMetric Metric { get; } = metric;
        public void Dispose()
        {
            lease.Dispose();
            loader.ReleaseOwnedPage(MetricCharge);
            cache.Evict(Metric.Tile);
        }
    }
}
