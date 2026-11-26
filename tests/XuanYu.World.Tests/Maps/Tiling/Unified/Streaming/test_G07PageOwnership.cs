using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07PageOwnership
{
    [G07DataFact]
    public async Task PageStaysChargedUntilUnpinnedAndEvicted()
    {
        var paths = new[] { Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")! };
        var catalog = new NasademTileCatalog(paths);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        await loader.Request(23, 120); await loader.Request(23, 121);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var tile = new GeoUnifiedTile(6, 529, 199);
        long before = loader.ReservedBytes;
        using (var lease = cache.Acquire(tile))
        {
            Assert.Equal(GeoSampleStatus.Valid, lease.Page.StatusAt(0, 0));
            Assert.True(loader.ReservedBytes > before);
            Assert.False(cache.Evict(tile));
        }
        Assert.True(cache.Evict(tile));
        Assert.Equal(before, loader.ReservedBytes);
    }
}
