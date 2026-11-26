using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07SampleStates
{
    [G07DataFact]
    public void SharedEdge_WaitsForPrioritySource_AndNeverChangesWithArrivalOrder()
    {
        string west = Environment.GetEnvironmentVariable("XYE_G07_WEST")!;
        string east = Environment.GetEnvironmentVariable("XYE_G07_EAST")!;
        Assert.True(File.Exists(west) && File.Exists(east));
        var catalog = new NasademTileCatalog([west, east]);
        var westTile = NasademHgtFile.Load(west);
        var eastTile = NasademHgtFile.Load(east);
        var source = new GeoUnifiedSamples(catalog, "fixture");
        int x = (180 + 121) * 3600, y = (90 + 23) * 3600 + 1000;
        Assert.Equal(GeoSampleStatus.NotLoaded, source.Sample(x, y).Status);
        source.Add(westTile);
        Assert.Equal(GeoSampleStatus.NotLoaded, source.Sample(x, y).Status);
        source.Add(eastTile);
        var result = source.Sample(x, y);
        Assert.Equal(GeoSampleStatus.Valid, result.Status);
        Assert.Equal(eastTile.HeightAt(0, 1000), result.Height);
        Assert.Equal("fixture", result.Version);
        Assert.Equal(GeoSampleStatus.OutsideCoverage, source.Sample(0, 0).Status);
    }
}
