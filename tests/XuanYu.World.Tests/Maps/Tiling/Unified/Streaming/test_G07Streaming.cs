using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07Streaming
{
    [G07DataFact]
    public async Task RepeatedRequestsShareOneLoadAndRespectBudget()
    {
        var paths = new[] { Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")! };
        var catalog = new NasademTileCatalog(paths);
        var loader = new GeoUnifiedTileLoader(catalog, 80_000_000);
        Task<bool> first = loader.Request(23, 120);
        Task<bool> duplicate = loader.Request(23, 120);
        Assert.Same(first, duplicate);
        Assert.True(await first);
        Assert.True(loader.ReservedBytes <= 80_000_000);
        Assert.Same(first, loader.Request(23, 120));
        Assert.False(await loader.Request(0, 0));
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        Assert.True(loader.Release(23, 120));
        Assert.Equal(0, loader.ReservedBytes);
        Assert.True(await first);
        Assert.Equal(GeoSampleStatus.NotLoaded, source.Sample(1083392, 407552).Status);
        Assert.True(await loader.Request(23, 120));
        Assert.Equal(GeoSampleStatus.Valid, source.Sample(1083392, 407552).Status);
    }

    [G07DataFact]
    public async Task InsufficientBudgetDoesNotLoadOrInventData()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 1000);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await loader.Request(23, 120));
        Assert.Equal(0, loader.ReservedBytes);
    }
}