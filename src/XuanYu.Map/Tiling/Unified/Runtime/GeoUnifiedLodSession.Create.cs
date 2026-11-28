using System.Diagnostics;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    public static Task<GeoUnifiedLodSession> CreateAsync(GeoUnifiedTile root,
        GeoUnifiedPageLoader pages, GeoUnifiedPageCache cache, GeoUnifiedTileLoader loader,
        GeoUnifiedSamples source, EnuFrame anchor, int resourceLimit = 128) =>
        CreateCoverageAsync(new GeoUnifiedCoverage(root), pages, cache, loader, source, anchor, resourceLimit);

    public static async Task<GeoUnifiedLodSession> CreateDatasetAsync(NasademTileCatalog catalog,
        EnuFrame anchor, int gpuCapacity, long budget = 1280L * 1024 * 1024, int resourceLimit = 2048,
        Egm96GeoidGrid? geoid = null)
    {
        var watch = Stopwatch.StartNew();
        var domain = new GeoUnifiedViewportDomain(catalog.Coordinates);
        if (domain.Roots.Count > gpuCapacity || domain.Roots.Count > resourceLimit)
            throw new InvalidOperationException("G07 完整根覆盖超过本轮页池容量。");
        var loader = new GeoUnifiedTileLoader(catalog, budget);
        long rootCharge = domain.Roots.Count * (GeoUnifiedPageCache.PageCharge + MetricCharge);
        if (!loader.CanFitPage(catalog.Count, rootCharge))
            throw new InvalidOperationException("G07 数据集超过本轮 CPU 驻留预算；大规模流式加载留待 R04。");
        short min = short.MaxValue, max = short.MinValue;
        foreach (var batch in catalog.Coordinates.Chunk(4))
        {
            var loaded = await Task.WhenAll(batch.Select(key =>
                loader.Request(key.South, key.West))).ConfigureAwait(false);
            if (loaded.Any(ok => !ok)) throw new InvalidDataException("G07 原始 HGT 加载失败。");
            foreach (var key in batch)
            {
                if (!loader.TryGet(key.South, key.West, out var tile))
                    throw new InvalidDataException("G07 原始 HGT 未驻留。");
                min = Math.Min(min, tile.MinElevation); max = Math.Max(max, tile.MaxElevation);
            }
        }
        double rawMs = watch.Elapsed.TotalMilliseconds;
        var source = new GeoUnifiedSamples(catalog, "g07-viewport", loader, domain, geoid);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache, retainCatalog: true);
        var session = await CreateCoverageAsync(new GeoUnifiedCoverage(domain.Roots), pages, cache,
            loader, source, anchor, resourceLimit, deferMetrics: true).ConfigureAwait(false);
        session._viewportHeights = (min, max);
        session.PreparationProfile = $"原始解码及校验={rawMs:F1}ms 根页准备={watch.Elapsed.TotalMilliseconds - rawMs:F1}ms";
        return session;
    }

    private static async Task<GeoUnifiedLodSession> CreateCoverageAsync(GeoUnifiedCoverage coverage,
        GeoUnifiedPageLoader pages, GeoUnifiedPageCache cache, GeoUnifiedTileLoader loader,
        GeoUnifiedSamples source, EnuFrame anchor, int resourceLimit, bool deferMetrics = false)
    {
        var session = new GeoUnifiedLodSession(coverage, pages, cache, loader, source, anchor, resourceLimit);
        try
        {
            foreach (var root in coverage.Leaves)
                await session.Start(root, coverage.EdgeMask(root), deferMetrics)!.ConfigureAwait(false);
            return session;
        }
        catch { session.Dispose(); throw; }
    }
}