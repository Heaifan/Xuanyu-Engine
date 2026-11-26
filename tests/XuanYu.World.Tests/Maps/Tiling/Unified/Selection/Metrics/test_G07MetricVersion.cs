using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07MetricVersion
{
    [G07DataFact]
    public async Task OldMetricCannotDescribeNewVersionPage()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 100_000_000);
        var oldSource = new GeoUnifiedSamples(catalog, "v1", loader);
        var oldCache = new GeoUnifiedPageCache(loader, oldSource);
        var oldPages = new GeoUnifiedPageLoader(catalog, loader, oldCache);
        var tile = new GeoUnifiedTile(0, 33861, 12736);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        var oldPage = await oldPages.RequestAsync(tile);
        var metric = GeoUnifiedLodMetric.Measure(oldPage.Page, oldSource, anchor, 0)!.Value;
        Assert.True(metric.Matches(oldPage.Page, 0));
        Assert.False(metric.Matches(oldPage.Page, 8));
        oldPage.Dispose(); Assert.True(oldCache.Evict(tile));
        Assert.True(loader.Release(23, 120)); Assert.True(loader.TryAdvanceVersion());
        var newSource = new GeoUnifiedSamples(catalog, "v2", loader);
        var newPages = new GeoUnifiedPageLoader(catalog, loader, new GeoUnifiedPageCache(loader, newSource));
        using var newPage = await newPages.RequestAsync(tile);
        Assert.False(metric.Matches(newPage.Page, 0));
    }
}
