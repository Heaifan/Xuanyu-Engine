using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05SourceResidency
{
    [Fact]
    public void IndexStartsEmpty_AndLruRetainsExactSamples()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        using var c = new G02BHgtFixture("N24E122");
        a.SetHeight(1800, 1800, 123);
        b.SetHeight(1800, 1800, 456);
        c.SetHeight(1800, 1800, 789);
        var catalog = new NasademTileCatalog([a.Path, b.Path, c.Path]);
        var source = new NasademElevationSource(catalog, 2);
        Assert.Equal(3, catalog.Entries.Count);
        Assert.Equal(0, source.Residency.ResidentCount);
        Assert.Equal(0L, source.Residency.LoadCount);
        Assert.Equal(123d, source.Sample(24.5, 120.5)!.Value);
        Assert.Equal(456d, source.Sample(24.5, 121.5)!.Value);
        Assert.Equal(123d, source.Sample(24.5, 120.5)!.Value);
        Assert.Equal(2L, source.Residency.LoadCount);
        Assert.Equal(789d, source.Sample(24.5, 122.5)!.Value);
        Assert.Equal(1L, source.Residency.EvictedCount);
        Assert.Equal(123d, source.Sample(24.5, 120.5)!.Value);
        Assert.Equal(3L, source.Residency.LoadCount);
        Assert.Equal(456d, source.Sample(24.5, 121.5)!.Value);
        Assert.Equal(4L, source.Residency.LoadCount);
        Assert.Equal(2L, source.Residency.EvictedCount);
        Assert.Equal(2, source.Residency.ResidentCount);
        Assert.Equal(2 * NasademTile.ByteLength, source.Residency.ResidentBytes);
        Assert.Null(source.Sample(24.5, 123.5));
        Assert.Equal(4L, source.Residency.LoadCount);
    }

    [Fact]
    public void SharedBoundaryAndNoDataRemainCorrectAfterEviction()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        a.SetHeight(3600, 1800, 111);
        b.SetHeight(0, 1800, 222);
        b.SetHeight(1800, 1800, short.MinValue);
        var source = new NasademElevationSource(new NasademTileCatalog([a.Path, b.Path]), 1);
        Assert.Equal(222d, source.Sample(24.5, 121d)!.Value);
        Assert.Null(source.Sample(24.5, 121.5));
        Assert.Equal(0d, source.Sample(24.5, 120.5)!.Value);
        Assert.Equal(222d, source.Sample(24.5, 121d)!.Value);
        Assert.Equal(1, source.ReadSummary(24, 121)!.Value.NoDataCount);
        Assert.Equal(1, source.Residency.ResidentCount);
        Assert.Equal(3L, source.Residency.LoadCount);
    }
}