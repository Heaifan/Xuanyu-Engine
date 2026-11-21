using System.Numerics;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06SurfaceNormal
{
    [Fact]
    public void FlatAndMissingSamples_UseOnlyKnownElevations()
    {
        using var fixture = new G02BHgtFixture("N24E121");
        fixture.SetHeight(1800, 1800, 100);
        fixture.SetHeight(1799, 1800, short.MinValue);
        fixture.SetHeight(1801, 1800, 130);
        fixture.SetHeight(1800, 1799, 100);
        fixture.SetHeight(1800, 1801, 100);
        fixture.SetHeight(1799, 900, short.MinValue);
        fixture.SetHeight(1801, 900, short.MinValue);
        var source = new NasademElevationSource(fixture.Load());

        Assert.Equal(Vector3.UnitZ, Assert.IsType<Vector3>(source.SampleSurfaceNormal(24.25, 121.25)));
        var n = Assert.IsType<Vector3>(source.SampleSurfaceNormal(24.5, 121.5));
        Assert.InRange(n.X, -0.75f, -0.70f);
        Assert.InRange(n.Y, -0.00001f, 0.00001f);
        Assert.InRange(n.Length(), 0.99999f, 1.00001f);
        Assert.Null(source.SampleSurfaceNormal(24.5, 121d + 1799d / 3600d));
        Assert.Null(source.SampleSurfaceNormal(24.25, 121.5));
    }

    [Fact]
    public void SharedHgtEdge_ReadsBothTilesForCentralDifference()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        west.SetHeight(3599, 1800, 100);
        west.SetHeight(3600, 1800, 100);
        east.SetHeight(0, 1800, 100);
        east.SetHeight(1, 1800, 160);
        east.SetHeight(0, 1799, 100);
        east.SetHeight(0, 1801, 100);
        var source = new NasademElevationSource(west.Load(), east.Load());

        var n = Assert.IsType<Vector3>(source.SampleSurfaceNormal(24.5, 121d));
        Assert.InRange(n.X, -0.75f, -0.70f);
        Assert.InRange(n.Y, -0.00001f, 0.00001f);
        Assert.InRange(n.Z, 0.65f, 0.71f);
        Assert.InRange(n.Length(), 0.99999f, 1.00001f);
    }

    [Fact]
    public void NorthRisingSlope_NormalFacesSouth()
    {
        using var fixture = new G02BHgtFixture("N24E121");
        fixture.SetHeight(1800, 1800, 100);
        fixture.SetHeight(1800, 1799, 70);
        fixture.SetHeight(1800, 1801, 130);
        fixture.SetHeight(1799, 1800, 100);
        fixture.SetHeight(1801, 1800, 100);
        var source = new NasademElevationSource(fixture.Load());

        var n = Assert.IsType<Vector3>(source.SampleSurfaceNormal(24.5, 121.5));
        Assert.InRange(n.X, -0.00001f, 0.00001f);
        Assert.InRange(n.Y, -0.72f, -0.68f);
        Assert.InRange(n.Z, 0.70f, 0.74f);
    }
}