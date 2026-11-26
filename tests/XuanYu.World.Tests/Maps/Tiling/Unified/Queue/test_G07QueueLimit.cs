using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07QueueLimit
{
    [G07DataFact]
    public async Task PendingQueueHasExplicitLimitWithoutBlockingCaller()
    {
        string west = Environment.GetEnvironmentVariable("XYE_G07_WEST")!;
        string east = Environment.GetEnvironmentVariable("XYE_G07_EAST")!;
        string north = Path.Combine(Path.GetDirectoryName(west)!, "NASADEM_HGT_n24e120.zip");
        var loader = new GeoUnifiedTileLoader(new NasademTileCatalog([west, east, north]), 80_000_000, 1);
        await loader.Request(23, 120);
        var waiting = loader.Request(23, 121);
        Assert.False(waiting.IsCompleted);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await loader.Request(24, 120));
        Assert.True(loader.Release(23, 120));
        Assert.NotNull(await waiting.WaitAsync(TimeSpan.FromSeconds(15)));
    }
}
