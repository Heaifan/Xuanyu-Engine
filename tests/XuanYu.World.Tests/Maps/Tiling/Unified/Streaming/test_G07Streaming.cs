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
        var loader = new GeoUnifiedTileLoader(new NasademTileCatalog(paths), 80_000_000);
        Task<NasademTile?> first = loader.Request(23, 120);
        Task<NasademTile?> duplicate = loader.Request(23, 120);
        Assert.Same(first, duplicate);
        Assert.NotNull(await first);
        Assert.True(loader.ReservedBytes <= 80_000_000);
        Assert.Same(await first, await loader.Request(23, 120));
        Assert.Null(await loader.Request(0, 0));
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
