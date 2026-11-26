using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07PageEviction
{
    [G07DataFact]
    public async Task NewPageEvictsUnusedRawTileWhenBudgetIsFull()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 80_000_000);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var west = new GeoUnifiedTile(0, 33861, 12736);
        var east = new GeoUnifiedTile(0, 33864, 12737);
        using (var first = await pages.RequestAsync(west))
            Assert.Equal(GeoSampleStatus.Valid, first.Page.StatusAt(0, 0));
        using (var second = await pages.RequestAsync(east).WaitAsync(TimeSpan.FromSeconds(5)))
            Assert.Equal(GeoSampleStatus.Valid, second.Page.StatusAt(0, 0));
        using var oldPage = await pages.RequestAsync(west);
        Assert.Equal(GeoSampleStatus.Valid, oldPage.Page.StatusAt(0, 0));
        Assert.True(loader.ReservedBytes <= 80_000_000);
    }
}
