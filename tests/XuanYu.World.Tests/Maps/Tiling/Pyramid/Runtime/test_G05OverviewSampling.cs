using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05OverviewSampling
{
    [Fact]
    public void Levels_ReadActualDerivedSamplesWithoutLoadingOriginalCache()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(60, 60, 600);
        hgt.SetHeight(72, 72, 1200);
        hgt.SetHeight(73, 73, 2345);
        var source = new NasademElevationSource(new NasademTileCatalog([hgt.Path]), 1, true);
        var far = source.CoarseFor(9);
        var middle = source.CoarseFor(10);
        Assert.Equal(1, source.OverviewCount);
        Assert.Equal(188644L, source.OverviewBytes);
        Assert.Equal(600d, far.Sample(24d + 60d / 3600d, 121d + 60d / 3600d)!.Value);
        Assert.InRange(far.Sample(24d + 72d / 3600d, 121d + 72d / 3600d)!.Value, 383.99, 384.01);
        Assert.Equal(1200d, middle.Sample(24d + 72d / 3600d, 121d + 72d / 3600d)!.Value);
        Assert.Equal(0L, source.Residency.LoadCount);
        Assert.Equal(2345d, source.Sample(24d + 73d / 3600d, 121d + 73d / 3600d)!.Value);
        Assert.Equal(1L, source.Residency.LoadCount);
    }

    [Fact]
    public void OverviewBoundary_PreservesExistingOwnershipAndNoData()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        west.SetHeight(3600, 1800, 111);
        east.SetHeight(0, 1800, 222);
        west.SetHeight(3600, 1860, 333);
        east.SetHeight(0, 1860, short.MinValue);
        var src = new NasademElevationSource(new NasademTileCatalog([west.Path, east.Path]), 1, true);
        foreach (int level in new[] { 9, 10 })
        {
            var view = src.CoarseFor(level);
            Assert.Equal(222d, view.Sample(24.5, 121d)!.Value);
            Assert.Null(view.Sample(24d + 1860d / 3600d, 121d));
            Assert.Null(view.Sample(0, 0));
        }
        Assert.Equal(0L, src.Residency.LoadCount);
    }
}