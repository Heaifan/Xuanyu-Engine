using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02CFallback
{
    [Fact]
    public void CompleteParent_IsServedWhileChildIsPending()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainTileSession(
            new NasademElevationSource(hgt.Load()), cellsPerEdge: 8);
        var parent = new GeoTileId(8, 427, 94);
        GeoTileId child = GeographicTileScheme.Children(parent)[0];
        session.Request(parent);
        Assert.Equal(1, session.ProcessPending(1));
        GeoHeightTile parentTile = session.Request(parent)!;
        Assert.Equal(81, parentTile.ValidSampleCount);

        Assert.Equal(parent, session.Request(child)!.Id);
        Assert.Equal(1, session.PendingCount);
        Assert.Equal(1, session.ProcessPending(1));
        Assert.Equal(child, session.Request(child)!.Id);
    }

    [Fact]
    public void ChildRequest_DoesNotImplicitlyBuildAllAncestors()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainTileSession(
            new NasademElevationSource(hgt.Load()), cellsPerEdge: 8);
        GeoTileId child = new(9, 854, 188);
        Assert.Null(session.Request(child));
        Assert.Equal(1, session.PendingCount);
        Assert.Equal(1, session.ProcessPending(1));
        Assert.Equal(1, session.CachedCount);
        Assert.Equal(child, session.Request(child)!.Id);
    }

    [Fact]
    public void PartialParent_IsNotMistakenForGlobalFallback()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainTileSession(
            new NasademElevationSource(hgt.Load()), cellsPerEdge: 8);
        GeoTileId parent = new(7, 213, 47);
        session.Request(parent);
        session.ProcessPending(1);
        GeoHeightTile loaded = session.Request(parent)!;
        Assert.InRange(loaded.ValidSampleCount, 1, 80);
        GeoTileId child = GeographicTileScheme.Children(parent)[0];
        Assert.Null(session.Request(child));
        Assert.Equal(1, session.PendingCount);
    }
}