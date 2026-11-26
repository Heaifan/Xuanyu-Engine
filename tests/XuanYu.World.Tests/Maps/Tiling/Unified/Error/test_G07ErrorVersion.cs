using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ErrorVersion
{
    [G07DataFact]
    public void ErrorRejectsPageFromDifferentDatasetEvenWithSameLabel()
    {
        string path = Environment.GetEnvironmentVariable("XYE_G07_WEST")!;
        var catalog = new NasademTileCatalog([path]);
        var first = new GeoUnifiedSamples(catalog, "v1");
        var second = new GeoUnifiedSamples(catalog, "v1");
        first.Add(NasademHgtFile.Load(path));
        second.Add(NasademHgtFile.Load(path));
        var page = GeoUnifiedHeightPage.Create(new GeoUnifiedTile(0, 33861, 12736), first);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        Assert.Throws<InvalidOperationException>(() => GeoUnifiedError.Measure(page, second, anchor, 0));
    }
}
