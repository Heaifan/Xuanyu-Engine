using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ReferenceEviction
{
    [G07DataFact]
    public async Task RawTileReloadedForMetricCanBeEvictedAfterPinEnds()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 80_000_000);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var pages = new GeoUnifiedPageLoader(catalog, loader, new GeoUnifiedPageCache(loader, source));
        var west = new GeoUnifiedTile(0, 33861, 12736);
        var east = new GeoUnifiedTile(0, 33864, 12737);
        using (await pages.RequestAsync(west)) { }
        using (await pages.RequestAsync(east)) { }
        using (await pages.PinReferenceAsync(west)) { }
        using var next = await pages.RequestAsync(new GeoUnifiedTile(0, 33865, 12737))
            .WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(GeoSampleStatus.Valid, next.Page.StatusAt(0, 0));
    }
}
