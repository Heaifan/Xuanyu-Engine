using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07PageQueue
{
    [G07DataFact]
    public async Task PageQueueRejectsExcessWhileFirstAwaitsBudget()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 80_000_000);
        await loader.Request(23, 120);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache, 1);
        var first = pages.RequestAsync(new GeoUnifiedTile(0, 33864, 12737));
        var duplicate = pages.RequestAsync(new GeoUnifiedTile(0, 33864, 12737));
        Assert.False(first.IsCompleted);
        Assert.False(duplicate.IsCompleted);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await pages.RequestAsync(new GeoUnifiedTile(0, 33865, 12737)));
        Assert.True(loader.Release(23, 120));
        using var lease = await first.WaitAsync(TimeSpan.FromSeconds(15));
        using var second = await duplicate.WaitAsync(TimeSpan.FromSeconds(15));
        Assert.Same(lease.Page, second.Page);
        Assert.Equal(GeoSampleStatus.Valid, lease.Page.StatusAt(0, 0));
    }
}
