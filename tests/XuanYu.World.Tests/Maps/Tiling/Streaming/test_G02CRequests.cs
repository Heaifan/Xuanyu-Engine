using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02CRequests
{
    [Fact]
    public void RepeatedRequest_IsQueuedOnce()
    {
        var session = new GeoTerrainTileSession(new NasademElevationSource());
        var id = new GeoTileId(8, 427, 94);
        Assert.Null(session.Request(id));
        Assert.Null(session.Request(id));
        Assert.Equal(1, session.PendingCount);
    }

    [Fact]
    public void ProcessingBudget_KeepsRequestOrder()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainTileSession(
            new NasademElevationSource(hgt.Load()), cellsPerEdge: 8);
        var first = new GeoTileId(8, 427, 94);
        var second = new GeoTileId(8, 428, 94);
        session.Request(first);
        session.Request(second);

        Assert.Equal(1, session.ProcessPending(1));
        Assert.Equal(first, session.Request(first)!.Id);
        Assert.Equal(1, session.PendingCount);
        Assert.Null(session.Request(second));
        Assert.Equal(1, session.ProcessPending(1));
        Assert.Equal(second, session.Request(second)!.Id);
        Assert.Equal(0, session.ProcessPending(1));
    }

    [Fact]
    public void EmptySource_IsCachedAsMissingInsteadOfZeroHeight()
    {
        var session = new GeoTerrainTileSession(new NasademElevationSource());
        var id = new GeoTileId(8, 0, 0);
        Assert.Null(session.Request(id));
        Assert.Equal(1, session.ProcessPending(1));
        Assert.Null(session.Request(id));
        Assert.Equal(0, session.PendingCount);
        Assert.Equal(1, session.CachedCount);
    }
}