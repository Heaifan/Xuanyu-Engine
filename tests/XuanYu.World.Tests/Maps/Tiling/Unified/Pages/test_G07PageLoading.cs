using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07PageLoading
{
    [G07DataFact]
    public async Task CachedPageCanBeReusedAfterRawTilesAreEvicted()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var cache = new GeoUnifiedPageCache(loader, new GeoUnifiedSamples(catalog, "v1", loader));
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var tile = new GeoUnifiedTile(6, 529, 199);
        using (var first = await pages.RequestAsync(tile))
            Assert.Equal(GeoSampleStatus.Valid, first.Page.StatusAt(0, 0));
        Assert.True(loader.Release(23, 120)); Assert.True(loader.Release(23, 121));
        long pageOnly = loader.ReservedBytes;
        using (var second = await pages.RequestAsync(tile))
            Assert.Equal(GeoSampleStatus.Valid, second.Page.StatusAt(0, 0));
        Assert.Equal(pageOnly, loader.ReservedBytes);
        Assert.True(cache.Evict(tile));
    }

    [G07DataFact]
    public async Task RequestLoadsSourcesAndKeepsPageChargedUntilAllUsersRelease()
    {
        var paths = new[] { Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")! };
        var catalog = new NasademTileCatalog(paths);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var tile = new GeoUnifiedTile(6, 529, 199);
        using var first = await pages.RequestAsync(tile).WaitAsync(TimeSpan.FromSeconds(15));
        using var second = await pages.RequestAsync(tile).WaitAsync(TimeSpan.FromSeconds(15));
        Assert.Same(first.Page, second.Page);
        Assert.Equal(GeoSampleStatus.Valid, first.Page.StatusAt(0, 0));
        Assert.False(cache.Evict(tile));
        Assert.True(loader.Release(23, 120)); Assert.True(loader.Release(23, 121));
        Assert.True(loader.ReservedBytes > 0);
        Assert.Equal(GeoSampleStatus.Valid, first.Page.StatusAt(0, 0));
        first.Dispose(); second.Dispose();
        Assert.True(cache.Evict(tile));
        Assert.Equal(0, loader.ReservedBytes);
    }
}
