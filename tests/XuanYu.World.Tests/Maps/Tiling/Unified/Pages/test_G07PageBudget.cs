using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07PageBudget
{
    [G07DataFact]
    public async Task PageWhoseSourcesCannotFitFailsRatherThanWaitingForever()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 80_000_000);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var pages = new GeoUnifiedPageLoader(catalog, loader, new GeoUnifiedPageCache(loader, source));
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await pages.RequestAsync(new GeoUnifiedTile(6, 529, 199)).WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(0, loader.ReservedBytes);
    }
}
