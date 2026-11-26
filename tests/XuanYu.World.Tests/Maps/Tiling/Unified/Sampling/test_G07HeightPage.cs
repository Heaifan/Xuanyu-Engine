using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07HeightPage
{
    [G07DataFact]
    public void Page_PreservesExactGlobalGridAndSourceStatus()
    {
        string west = Environment.GetEnvironmentVariable("XYE_G07_WEST")!;
        string east = Environment.GetEnvironmentVariable("XYE_G07_EAST")!;
        var catalog = new NasademTileCatalog([west, east]);
        var source = new GeoUnifiedSamples(catalog, "v1");
        var tile = new GeoUnifiedTile(6, 529, 199);
        var pending = GeoUnifiedHeightPage.Create(tile, source);
        Assert.Equal(GeoSampleStatus.NotLoaded, pending.StatusAt(0, 0));
        source.Add(NasademHgtFile.Load(west));
        source.Add(NasademHgtFile.Load(east));
        var page = GeoUnifiedHeightPage.Create(tile, source);
        for (int row = 0; row < 33; row++)
            for (int col = 0; col < 33; col++)
            {
                var sample = source.Sample(tile.West + col * 64, tile.South + row * 64);
                Assert.Equal(GeoSampleStatus.Valid, page.StatusAt(col, row));
                Assert.Equal(sample.Height, page.HeightAt(col, row));
            }
    }
}
