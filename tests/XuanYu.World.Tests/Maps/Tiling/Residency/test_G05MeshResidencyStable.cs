using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05MeshResidencyStable
{
    [Fact]
    public void StationaryIrregularSet_ReusesActualDrawMeshes()
    {
        using var a = new G02BHgtFixture("N23E120");
        using var b = new G02BHgtFixture("N23E121");
        using var c = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(a.Load(), b.Load(), c.Load());
        session.ToggleNative();
        session.Camera.FrameTop(new GeoCoordinate(24d, 121d, 0d), 300_000d);
        for (int i = 0; i < 80 && (i == 0 || session.PendingCount > 0); i++)
            session.Draws(1000, 700);
        Assert.Equal(0, session.PendingCount);
        var before = session.Draws(1000, 700).ToArray();
        GeoTerrainMeshCacheStats stats = session.MeshCacheStats;
        for (int frame = 0; frame < 5; frame++)
        {
            var after = session.Draws(1000, 700);
            Assert.NotEmpty(after);
            Assert.Equal(before.Length, after.Count);
            for (int i = 0; i < before.Length; i++)
                Assert.Same(before[i].Mesh, after[i].Mesh);
            Assert.Equal(stats, session.MeshCacheStats);
        }
    }
}