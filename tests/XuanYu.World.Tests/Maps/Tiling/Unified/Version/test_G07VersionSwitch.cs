using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07VersionSwitch
{
    [G07DataFact]
    public async Task VersionCannotAdvanceWhileOldPageIsResident()
    {
        var paths = new[] { Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")! };
        var catalog = new NasademTileCatalog(paths);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var old = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, old);
        var tile = new GeoUnifiedTile(6, 529, 199);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        using var lease = await pages.RequestAsync(tile).WaitAsync(TimeSpan.FromSeconds(15));
        Assert.False(loader.TryAdvanceVersion());
        Assert.True(loader.Release(23, 120)); Assert.True(loader.Release(23, 121));
        Assert.False(loader.TryAdvanceVersion());
        Assert.Equal(GeoSampleStatus.Valid, lease.Page.StatusAt(0, 0));
        lease.Dispose(); Assert.True(cache.Evict(tile));
        Assert.True(loader.TryAdvanceVersion());
        Assert.Throws<InvalidOperationException>(() => old.Sample(tile.West, tile.South));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await pages.RequestAsync(tile));
        Assert.Equal(0, loader.ReservedBytes);
        var next = new GeoUnifiedSamples(catalog, "v2", loader);
        await loader.Request(23, 120); await loader.Request(23, 121);
        Assert.Equal("v2", next.Sample(tile.West, tile.South).Version);
    }
}
