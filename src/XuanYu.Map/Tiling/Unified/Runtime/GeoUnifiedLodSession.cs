using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession : IDisposable
{
    private readonly GeoUnifiedPageLoader _pages;
    private readonly GeoUnifiedPageCache _cache;
    private readonly GeoUnifiedTileLoader _loader;
    private readonly GeoUnifiedSamples _source;
    private readonly EnuFrame _anchor;
    private readonly GeoUnifiedSelector _selector = new();
    private readonly Dictionary<(GeoUnifiedTile Tile, int Mask), Task<Resource>> _resources = [];
    private readonly Dictionary<(GeoUnifiedTile Tile, int Mask), long> _retryAfter = [];
    private readonly SemaphoreSlim _metricWorker = new(1, 1);
    private readonly int _resourceLimit;
    private bool _disposed;
    public GeoUnifiedCoverage Coverage { get; private set; }
    public string? Diagnostic { get; private set; }
    public string? PreparationProfile { get; private set; }
    public GeoUnifiedViewportDomain? Domain => _source.Domain;

    private GeoUnifiedLodSession(GeoUnifiedCoverage coverage, GeoUnifiedPageLoader pages,
        GeoUnifiedPageCache cache, GeoUnifiedTileLoader loader, GeoUnifiedSamples source,
        EnuFrame anchor, int resourceLimit)
    {
        if (!ReferenceEquals(pages.Loader, loader) || !ReferenceEquals(pages.Cache, cache) ||
            !ReferenceEquals(source.Loader, loader)) throw new ArgumentException("地形会话必须共用同一资源所有者。");
        if (resourceLimit < 5) throw new ArgumentOutOfRangeException(nameof(resourceLimit));
        Coverage = coverage;
        _pages = pages; _cache = cache; _loader = loader; _source = source;
        _anchor = anchor; _resourceLimit = resourceLimit;
    }

    public GeoUnifiedSelection Step(EnuPosition eye, int height, double fov,
        Func<GeoUnifiedTile, int, bool> gpuReady, Func<GeoUnifiedTile, bool>? visible = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Diagnostic = null;
        var domain = Domain;
        double? Error(GeoUnifiedTile tile, int mask)
        {
            if (domain is not null && tile.Level > GeoUnifiedViewportDomain.RootLevel)
                return double.PositiveInfinity;
            if (domain is not null && visible is not null && !Visible(tile)) return 0d;
            if (!TryResource(tile, mask, out var item)) return null;
            if (item.TryMetric(out var metric)) return metric.ScreenError(eye, height, fov);
            Diagnostic = item.Failure ?? "根页指标后台准备中";
            return null;
        }
        bool Ready(GeoUnifiedTile tile, int mask) => TryResource(tile, mask, out _) && gpuReady(tile, mask);
        bool Visible(GeoUnifiedTile tile) => (domain is null || domain.Intersects(tile)) &&
            (visible is null || visible(tile));
        var result = _selector.StepWithMetrics(Coverage, Error, Ready, Visible);
        Coverage = result.Coverage;
        Prune(result);
        return result;
    }

    public GeoUnifiedPageCache.PageLease? AcquirePage(GeoUnifiedTile tile, int mask)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return TryResource(tile, mask, out _) ? _cache.TryAcquire(tile) : null;
    }

    private bool TryResource(GeoUnifiedTile tile, int mask, out Resource resource)
    {
        var task = Start(tile, mask);
        if (task is { IsCompletedSuccessfully: true }) { resource = task.Result; return true; }
        if (task is { IsFaulted: true })
        {
            Diagnostic = task.Exception?.GetBaseException().Message;
            var key = (tile, mask);
            long now = Environment.TickCount64;
            if (!_retryAfter.TryGetValue(key, out long retry)) _retryAfter[key] = now + 1000;
            else if (now >= retry)
            {
                _resources.Remove(key);
                _retryAfter.Remove(key);
                Retire(task);
                _ = Start(tile, mask);
            }
        }
        resource = null!;
        return false;
    }
}