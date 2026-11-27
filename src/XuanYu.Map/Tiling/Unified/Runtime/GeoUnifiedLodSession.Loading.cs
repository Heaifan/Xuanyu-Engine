namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    private const long MetricCharge = 512;

    private Task<Resource>? Start(GeoUnifiedTile tile, int mask, bool deferMetric = false)
    {
        var key = (tile, mask);
        if (_resources.TryGetValue(key, out var task)) return task;
        if (_resources.Count >= _resourceLimit)
        {
            Diagnostic = "地形指标资源达到上限。";
            return null;
        }
        task = PrepareAsync(tile, mask, deferMetric);
        _resources.Add(key, task);
        return task;
    }

    private async Task<Resource> PrepareAsync(GeoUnifiedTile tile, int mask, bool deferMetric)
    {
        var lease = await _pages.RequestAsync(tile).ConfigureAwait(false);
        bool charged = false;
        try
        {
            if (!_loader.TryReserveOwnedPage(MetricCharge))
                throw new InvalidOperationException("地形指标预算不足。");
            charged = true;
            if (deferMetric)
                return new Resource(lease, GeoUnifiedBounds.FromPage(lease.Page, _anchor),
                    () => MeasureAsync(lease, tile, mask), _cache, _loader);
            var metric = await MeasureAsync(lease, tile, mask).ConfigureAwait(false);
            return new Resource(lease, metric, _cache, _loader);
        }
        catch
        {
            if (charged) _loader.ReleaseOwnedPage(MetricCharge);
            lease.Dispose();
            _cache.Evict(tile);
            throw;
        }
    }

    private async Task<GeoUnifiedLodMetric> MeasureAsync(GeoUnifiedPageCache.PageLease lease,
        GeoUnifiedTile tile, int mask)
    {
        await _metricWorker.WaitAsync().ConfigureAwait(false);
        try
        {
            using var pin = await _pages.PinReferenceAsync(tile).ConfigureAwait(false);
            var metric = await Task.Run(() => GeoUnifiedLodMetric.Measure(
                lease.Page, _source, _anchor, mask)).ConfigureAwait(false);
            return metric ?? throw new InvalidOperationException("参考高程未完整就绪。");
        }
        finally { _metricWorker.Release(); }
    }
}