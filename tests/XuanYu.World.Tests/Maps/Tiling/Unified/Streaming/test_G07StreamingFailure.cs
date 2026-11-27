using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07StreamingFailure
{
    [G07DataFact]
    public async Task FailedLoadReleasesReservationAndCanBeRetried()
    {
        string folder = Path.Combine(Path.GetTempPath(), "XYE-G07-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "NASADEM_HGT_n23e120.zip");
        string original = Environment.GetEnvironmentVariable("XYE_G07_WEST")!;
        try
        {
            File.Copy(original, path);
            var loader = new GeoUnifiedTileLoader(new NasademTileCatalog([path]), 80_000_000);
            File.Delete(path);
            await Assert.ThrowsAnyAsync<IOException>(async () => await loader.Request(23, 120));
            Assert.Equal(0, loader.ReservedBytes);
            File.Copy(original, path);
            Assert.True(await loader.Request(23, 120));
        }
        finally { Directory.Delete(folder, true); }
    }
}