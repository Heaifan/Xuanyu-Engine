using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07Ownership
{
    [G07DataFact]
    public async Task ResidentLeaseBlocksReleaseAndPageSurvivesEviction()
    {
        var paths = new[] { Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")! };
        var catalog = new NasademTileCatalog(paths);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        await loader.Request(23, 120);
        await loader.Request(23, 121);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        GeoUnifiedHeightPage page;
        using (var lease = loader.PinResident())
        {
            Assert.False(loader.Release(23, 120));
            page = GeoUnifiedHeightPage.Create(new GeoUnifiedTile(6, 529, 199), source);
            Assert.Equal(GeoSampleStatus.Valid, page.StatusAt(0, 0));
        }
        short? saved = page.HeightAt(0, 0);
        Assert.True(loader.Release(23, 120));
        Assert.Equal(saved, page.HeightAt(0, 0));
        Assert.Equal(GeoSampleStatus.NotLoaded,
            source.Sample(1083392, 407552).Status);
    }
}
