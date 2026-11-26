using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07PageLifecycle
{
    [G07DataFact]
    public async Task MissingPageDoesNotConsumeBudget_AndRepeatedLifecycleDoesNotAccumulate()
    {
        var paths = new[] { Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")! };
        var catalog = new NasademTileCatalog(paths);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var tile = new GeoUnifiedTile(6, 529, 199);
        Assert.Throws<InvalidOperationException>(() => cache.Acquire(tile));
        Assert.Equal(0, loader.ReservedBytes);
        for (int i = 0; i < 3; i++)
        {
            await loader.Request(23, 120); await loader.Request(23, 121);
            using (var page = cache.Acquire(tile))
                Assert.Equal(GeoSampleStatus.Valid, page.Page.StatusAt(0, 0));
            Assert.True(cache.Evict(tile));
            Assert.True(loader.Release(23, 120)); Assert.True(loader.Release(23, 121));
            Assert.Equal(0, loader.ReservedBytes);
        }
    }
}
