using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05HeightPyramid
{
    [Fact]
    public void RealHgt_ProducesNestedGridsWithExactRetainedSamples()
    {
        using var file = new G02BHgtFixture("N24E121");
        file.SetHeight(0, 0, -43);
        file.SetHeight(3600, 3600, 3867);
        file.SetHeight(60, 120, 725);
        file.SetHeight(120, 60, short.MinValue);
        file.SetHeight(12, 24, 345);
        NasademTile tile = file.Load();
        var far = NasademOverviewBuilder.Build(tile, NasademOverviewLevel.Far);
        var middle = NasademOverviewBuilder.Build(tile, NasademOverviewLevel.Middle);
        Assert.Equal(61, far.Side);
        Assert.Equal(301, middle.Side);
        Assert.Equal(7442L, far.SampleBytes);
        Assert.Equal(181202L, middle.SampleBytes);
        Assert.Equal((short)-43, far.HeightAt(0, 0));
        Assert.Equal((short)3867, middle.HeightAt(300, 300));
        Assert.Equal((short)725, far.HeightAt(1, 2));
        Assert.Equal(far.HeightAt(1, 2), middle.HeightAt(5, 10));
        Assert.Null(far.HeightAt(2, 1));
        Assert.Null(middle.HeightAt(10, 5));
        Assert.Equal((short)345, middle.HeightAt(1, 2));
        Assert.Equal((short)345, tile.HeightAt(12, 24));
    }

    [Theory]
    [InlineData(NasademOverviewLevel.Far, 61)]
    [InlineData(NasademOverviewLevel.Middle, 301)]
    public void MatchingSourceEdges_KeepMatchingDerivedEdges(NasademOverviewLevel level, int side)
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        west.SetHeight(3600, 1800, 555);
        east.SetHeight(0, 1800, 555);
        var a = NasademOverviewBuilder.Build(west.Load(), level);
        var b = NasademOverviewBuilder.Build(east.Load(), level);
        Assert.Equal((short)555, a.HeightAt(side - 1, side / 2));
        for (int row = 0; row < side; row++)
            Assert.Equal(a.HeightAt(side - 1, row), b.HeightAt(0, row));
    }
}