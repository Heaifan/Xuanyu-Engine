using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06NativeCache
{
    [Fact]
    public void Insert_DoesNotEvictKeysActiveInCurrentSelection()
    {
        var cache = new GeoTerrainNativeMeshCache<int, int>(3);
        cache.BeginFrame([1, 2, 3]);
        cache.Set(1, 10);
        cache.Set(2, 20);
        cache.Set(3, 30);
        cache.BeginFrame([1, 2, 4]);

        cache.Set(4, 40);

        Assert.Equal(10, cache.Get(1));
        Assert.Equal(20, cache.Get(2));
        Assert.Equal(40, cache.Get(4));
        Assert.Equal(1, cache.EvictedCount);
        Assert.Equal(0, cache.ActiveEvictionCount);
    }

    [Fact]
    public void TopologyIdentityChange_IsAMissAndCannotReuseOldMesh()
    {
        var cache = new GeoTerrainNativeMeshCache<(GeoTileId Id, object Topology), int>(2);
        object before = new();
        object after = new();
        cache.BeginFrame([(new GeoTileId(13, 1, 1), before)]);
        cache.Set((new GeoTileId(13, 1, 1), before), 10);
        cache.BeginFrame([(new GeoTileId(13, 1, 1), after)]);

        Assert.False(cache.TryGet((new GeoTileId(13, 1, 1), after), out _));
        Assert.True(cache.TryGet((new GeoTileId(13, 1, 1), before), out int old));
        Assert.Equal(10, old);
    }

    [Fact]
    public void Draws_WarmCacheSurvivesShortRoundTrip()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        var focus = new GeoCoordinate(24.5, 121.5, 0);
        DrawUntilWarm(focus);
        Assert.True(session.NativeFirstBuildCount > 0);

        DrawUntilWarm(new GeoCoordinate(24.505, 121.505, 0));
        long shiftedBuilds = session.NativeFirstBuildCount;
        long shiftedTopologyMisses = session.NativeTopologyMissCount;
        DrawUntilWarm(focus);

        Assert.True(session.NativeFirstBuildCount - shiftedBuilds <=
            session.NativeTopologyMissCount - shiftedTopologyMisses,
            "往返触发了与拓扑身份变化无关的 Native 重建。");
        Assert.Equal(0, session.NativeActiveEvictionCount);
        Assert.True(session.NativeCacheCapacityLimit >= session.NativeRequestedCount);

        void DrawUntilWarm(GeoCoordinate target)
        {
            session.Camera.FrameTop(target, 5000);
            for (int i = 0; i < 180; i++)
            {
                session.Draws(776, 532);
                if (session.PendingCount == 0 && session.NativeDrawCount ==
                    session.NativeRequestedCount && session.NativeRequestedCount > 0)
                    return;
            }
            Assert.True(session.NativeDrawCount == session.NativeRequestedCount &&
                session.NativeRequestedCount > 0, "生产 Draws 调度在预算内未进入 Native 暖态。");
        }
    }
}
