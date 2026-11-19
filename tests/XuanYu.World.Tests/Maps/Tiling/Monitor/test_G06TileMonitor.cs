using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06TileMonitor
{
    [Fact]
    public void Snapshot_ReadsRealTilesWithoutChangingCache()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.ToggleNative();
        session.Camera.FrameTop(new GeoCoordinate(24.5d, 121.5d, 0d), 1000d);
        session.Draws(1000, 700);
        GeoTerrainMeshCacheStats before = session.MeshCacheStats;
        var a = session.ReadMonitor();
        var b = session.ReadMonitor();
        Assert.Equal(before, session.MeshCacheStats);
        Assert.Equal(a.Frame, b.Frame);
        Assert.Equal(session.SelectedCount, a.TargetCount);
        Assert.Equal(session.PendingCount, a.PendingCount);
        Assert.Equal(session.VisibleTileIds.Count, a.VisibleCount);
        Assert.Equal(a.VisibleCount, a.Tiles.Count(t => t.Drawn));
        Assert.Equal(a.TargetCount, a.Tiles.Count(t => t.Targeted));
        Assert.All(session.VisibleTileIds, id =>
            Assert.Contains(a.Tiles, t => t.Id == id && t.Drawn));
    }

    [Fact]
    public void PendingTiles_ReportActualParentCoverage()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.ToggleNative();
        session.Camera.FrameTop(new GeoCoordinate(24.5d, 121.5d, 0d), 1000d);
        session.Draws(1000, 700);
        var snapshot = session.ReadMonitor();
        var visible = snapshot.Tiles.Where(t => t.Drawn).Select(t => t.Id).ToHashSet();
        foreach (var t in snapshot.Tiles.Where(t => t.Targeted && !t.Drawn))
        {
            if (t.Fallback is not { } parent) continue;
            Assert.Contains(parent, visible);
            GeoTileId? current = GeographicTileScheme.Parent(t.Id);
            while (current != parent)
            {
                Assert.NotNull(current);
                current = GeographicTileScheme.Parent(current.Value);
            }
        }
    }
}