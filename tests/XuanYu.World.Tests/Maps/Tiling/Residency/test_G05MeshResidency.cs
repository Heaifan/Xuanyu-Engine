using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05MeshResidency
{
    [Fact]
    public void NearFarNear_EvictsColdMeshesWithoutDroppingCoverage()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.ToggleNative();
        var center = new GeoCoordinate(24.5d, 121.5d, 0d);
        session.Camera.FrameTop(center, 1000d);
        for (int i = 0; i < 160 && session.MeshCacheStats.BaseCount <= 100; i++)
            CheckFrame(session);
        int before = session.MeshCacheStats.BaseCount;
        Assert.True(before > 100, $"近景未建立足够网格：{before}");
        long evicted = session.MeshCacheStats.EvictedCount;

        session.Camera.FrameTop(center, 20_000_000d);
        for (int i = 0; i < 4; i++) CheckFrame(session);
        Assert.True(session.MeshCacheStats.EvictedCount > evicted);
        Assert.True(session.MeshCacheStats.BaseCount < before);

        session.Camera.FrameTop(center, 1000d);
        for (int i = 0; i < 40; i++) CheckFrame(session);
    }

    private static void CheckFrame(GeoTerrainRenderSession session)
    {
        var draws = session.Draws(1000, 700);
        Assert.NotEmpty(draws);
        GeoTerrainMeshCacheStats stats = session.MeshCacheStats;
        Assert.InRange(stats.BaseInactiveCount, 0, 64);
        Assert.InRange(stats.StitchedInactiveCount, 0, 64);
        var ids = session.VisibleTileIds.ToHashSet();
        Assert.Equal(ids.Count, draws.Count);
        double area = 0d;
        foreach (GeoTileId id in ids)
        {
            for (GeoTileId? p = GeographicTileScheme.Parent(id);
                p is GeoTileId parent; p = GeographicTileScheme.Parent(parent))
                Assert.DoesNotContain(parent, ids);
            GeoTileBounds b = GeographicTileScheme.Bounds(id);
            area += (Math.Min(25d, b.NorthLatitudeDegrees) -
                Math.Max(24d, b.SouthLatitudeDegrees)) *
                (Math.Min(122d, b.EastLongitudeDegrees) -
                Math.Max(121d, b.WestLongitudeDegrees));
        }
        Assert.InRange(area, 1d - 1e-9d, 1d + 1e-9d);
        Assert.True(stats.BaseActiveCount >= ids.Count);
    }
}