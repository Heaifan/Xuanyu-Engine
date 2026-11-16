using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02BSource
{
    [Fact]
    public void ExactGridPoint_RetainsOriginalHeight()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        hgt.SetHeight(1800, 1800, 1234);
        var source = new NasademElevationSource(hgt.Load());
        Assert.Equal(1234d, source.Sample(23.5, 120.5)!.Value);
        Assert.Null(source.Sample(0d, 0d));
    }

    [Fact]
    public void BilinearSampling_UsesFourRealSamples()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        hgt.SetHeight(900, 900, 100);
        hgt.SetHeight(901, 900, 200);
        hgt.SetHeight(900, 901, 300);
        hgt.SetHeight(901, 901, 400);
        var source = new NasademElevationSource(hgt.Load());
        double? height = source.Sample(23d + 900.5d / 3600d,
            120d + 900.5d / 3600d);
        Assert.NotNull(height);
        Assert.InRange(Math.Abs(height.Value - 250d), 0d, 1e-7d);
    }

    [Fact]
    public void NoData_RemainsMissingRatherThanZero()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        hgt.SetHeight(1800, 1800, short.MinValue);
        var source = new NasademElevationSource(hgt.Load());
        Assert.Null(source.Sample(23.5, 120.5));
        Assert.Null(source.Sample(23.5 + 0.25 / 3600d, 120.5));
        Assert.Equal(0d, source.Sample(23.5 + 2d / 3600d, 120.5)!.Value);
    }

    [Fact]
    public void SharedSourceBoundary_PrefersEastTileWhenBothLoaded()
    {
        using var west = new G02BHgtFixture("N23E120");
        using var east = new G02BHgtFixture("N23E121");
        west.SetHeight(3600, 1800, 111);
        east.SetHeight(0, 1800, 222);
        NasademTile westTile = west.Load();
        var source = new NasademElevationSource(westTile, east.Load());
        var westOnly = new NasademElevationSource(westTile);
        Assert.Equal(222d, source.Sample(23.5, 121d)!.Value);
        Assert.Equal(111d, westOnly.Sample(23.5, 121d)!.Value);
    }

    [Fact]
    public void DateLine_FallsBackToWestEdgeOfE179()
    {
        using var hgt = new G02BHgtFixture("N10E179");
        hgt.SetHeight(3600, 1800, 444);
        var source = new NasademElevationSource(hgt.Load());
        Assert.Equal(444d, source.Sample(10.5, 180d)!.Value);
        Assert.Equal(444d, source.Sample(10.5, -180d)!.Value);
    }
}