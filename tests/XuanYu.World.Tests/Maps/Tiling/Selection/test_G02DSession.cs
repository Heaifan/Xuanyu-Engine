using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02DSession
{
    [Fact]
    public void SelectedTiles_CanBeRequestedAndProcessedByRealSession()
    {
        var view = new GeoTileView(
            new EcefPosition(3d * Wgs84.SemiMajorAxisMeters, 0, 0),
            new EcefPosition(0, 0, 0), Math.PI / 3d, 800, 600);
        var selector = new GeoTileSelector(10_000d, 10d,
            maxLevel: 2, maxTiles: 8);
        var session = new GeoTerrainTileSession(new NasademElevationSource(),
            cellsPerEdge: 2, cacheCapacity: 8);
        IReadOnlyList<GeoTileId> selected = selector.Select(view);
        foreach (GeoTileId id in selected) session.Request(id);

        Assert.NotEmpty(selected);
        Assert.Equal(selected.Count, session.PendingCount);
        Assert.Equal(1, session.ProcessPending(1));
        Assert.Equal(selected.Count - 1, session.PendingCount);
        Assert.Equal(1, session.CachedCount);
    }

    [Fact]
    public void RepeatedSelection_DoesNotDuplicatePendingRequests()
    {
        var view = new GeoTileView(
            new EcefPosition(3d * Wgs84.SemiMajorAxisMeters, 0, 0),
            new EcefPosition(0, 0, 0), Math.PI / 3d, 800, 600);
        var selector = new GeoTileSelector(10_000d, 10d,
            maxLevel: 2, maxTiles: 8);
        var session = new GeoTerrainTileSession(new NasademElevationSource());
        IReadOnlyList<GeoTileId> selected = selector.Select(view);
        foreach (GeoTileId id in selected) session.Request(id);
        foreach (GeoTileId id in selected) session.Request(id);

        Assert.Equal(selected.Count, session.PendingCount);
    }
}