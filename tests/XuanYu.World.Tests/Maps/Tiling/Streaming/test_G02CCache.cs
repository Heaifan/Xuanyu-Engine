using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02CCache
{
    [Fact]
    public void Capacity_EvictsOldestTileAndAllowsReRequest()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainTileSession(
            new NasademElevationSource(hgt.Load()), 8, 2);
        GeoTileId first = new(8, 427, 94);
        GeoTileId second = new(8, 428, 94);
        GeoTileId third = new(8, 426, 94);
        foreach (GeoTileId id in new[] { first, second, third })
        {
            session.Request(id);
            Assert.Equal(1, session.ProcessPending(1));
        }

        Assert.Equal(2, session.CachedCount);
        Assert.Equal(second, session.Request(second)!.Id);
        Assert.Null(session.Request(first));
        Assert.Equal(1, session.PendingCount);
    }

    [Fact]
    public void CacheHit_DoesNotScheduleAnotherBuild()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainTileSession(
            new NasademElevationSource(hgt.Load()), 8);
        var id = new GeoTileId(8, 427, 94);
        session.Request(id);
        session.ProcessPending(1);
        GeoHeightTile first = session.Request(id)!;
        Assert.Same(first, session.Request(id));
        Assert.Equal(0, session.PendingCount);
        Assert.Equal(0, session.ProcessPending(1));
    }

    [Fact]
    public void PublicEntry_RejectsInvalidBudgets()
    {
        var source = new NasademElevationSource();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTerrainTileSession(source, cellsPerEdge: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTerrainTileSession(source, cacheCapacity: 0));
        var session = new GeoTerrainTileSession(source);
        Assert.Throws<ArgumentOutOfRangeException>(() => session.ProcessPending(0));
    }
}