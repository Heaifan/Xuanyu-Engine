using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07Residency
{
    [G07DataFact]
    public async Task WaitingRequestResumesAfterExplicitReleaseAndAccountsPages()
    {
        var paths = new[] { Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")! };
        var loader = new GeoUnifiedTileLoader(new NasademTileCatalog(paths), 80_000_000);
        await loader.Request(23, 120);
        Assert.True(loader.TryReservePage(1000));
        Assert.True(loader.ReservedBytes >= NasademTile.ByteLength * 3 + 1000);
        Task<NasademTile?> pending = loader.Request(23, 121);
        Assert.False(pending.IsCompleted);
        Assert.True(loader.Release(23, 120));
        Assert.NotNull(await pending.WaitAsync(TimeSpan.FromSeconds(15)));
        loader.ReleasePage(1000);
        Assert.True(loader.Release(23, 121));
        Assert.Equal(0, loader.ReservedBytes);
    }
}
