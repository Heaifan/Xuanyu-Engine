using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07RealCoverageFlow
{
    [G07DataFact]
    public async Task RootRemainsUntilFourRealChildPagesAreReady()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var root = new GeoUnifiedTile(6, 529, 199);
        var coverage = new GeoUnifiedCoverage(root);
        var selector = new GeoUnifiedSelector();
        using var parent = await pages.RequestAsync(root);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        var metric = GeoUnifiedLodMetric.Measure(parent.Page, source, anchor, 0)!.Value;
        var bounds = metric.Bounds;
        var eye = new EnuPosition((bounds.Min.East + bounds.Max.East) / 2,
            (bounds.Min.North + bounds.Max.North) / 2, bounds.Max.Up + 10_000);
        double pixels = metric.ScreenError(eye, 1080, Math.PI / 3);
        Assert.True(pixels > 2.5);
        var ready = new HashSet<GeoUnifiedTile> { root };
        var waiting = selector.StepWithEdges(coverage, _ => pixels, (tile, _) => ready.Contains(tile));
        Assert.Same(coverage, waiting.Coverage);
        var leases = new List<GeoUnifiedPageCache.PageLease>();
        try
        {
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                {
                    var child = root.Child(x, y);
                    leases.Add(await pages.RequestAsync(child));
                    ready.Add(child);
                }
            var committed = selector.StepWithEdges(coverage, _ => pixels, (tile, mask) =>
                ready.Contains(tile) && mask == 0);
            Assert.Equal(4, committed.Coverage.Leaves.Count);
            Assert.DoesNotContain(root, committed.Coverage.Leaves);
            Assert.All(committed.Coverage.Leaves, tile => Assert.Contains(tile, ready));
        }
        finally { foreach (var lease in leases) lease.Dispose(); }
    }
}
