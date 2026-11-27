using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07OccupiedPageBudget
{
    [G07DataFact]
    public async Task OccupiedPagesRejectImpossibleRequestAndReleaseAllowsRetry()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        const long pageCharge = 33L * 33 * 64;
        long budget = 6L * NasademTile.ByteLength + pageCharge;
        var loader = new GeoUnifiedTileLoader(catalog, budget);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var west = new GeoUnifiedTile(0, 33861, 12736);
        var next = new GeoUnifiedTile(0, 33860, 12736);
        var root = new GeoUnifiedTile(6, 529, 199);
        using (await pages.RequestAsync(west))
        using (await pages.RequestAsync(next))
        {
            long before = loader.ReservedBytes;
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await pages.RequestAsync(root).WaitAsync(TimeSpan.FromSeconds(5)));
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await pages.PinReferenceAsync(root).WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(before, loader.ReservedBytes);
        }
        Assert.True(cache.Evict(west));
        Assert.True(cache.Evict(next));
        using var lease = await pages.RequestAsync(root).WaitAsync(TimeSpan.FromSeconds(15));
        Assert.Equal(GeoSampleStatus.Valid, lease.Page.StatusAt(0, 0));
        using var pin = await pages.PinReferenceAsync(root).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(budget, loader.ReservedBytes);
    }
}