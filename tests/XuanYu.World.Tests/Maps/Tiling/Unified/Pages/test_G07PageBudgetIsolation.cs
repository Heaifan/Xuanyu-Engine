using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07PageBudgetIsolation
{
    [G07DataFact]
    public async Task ManualReleaseCannotUnchargeAnInUseCachedPage()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 100_000_000);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        using var page = await pages.RequestAsync(new GeoUnifiedTile(0, 33861, 12736));
        long before = loader.ReservedBytes;
        Assert.Throws<ArgumentOutOfRangeException>(() => loader.ReleasePage(33L * 33 * 64));
        Assert.Equal(before, loader.ReservedBytes);
        Assert.Equal(GeoSampleStatus.Valid, page.Page.StatusAt(0, 0));
    }
}
