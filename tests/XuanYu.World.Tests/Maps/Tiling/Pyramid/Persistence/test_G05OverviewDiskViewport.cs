using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05OverviewDiskViewport
{
    [Fact]
    public void ReopenedSession_UsesPersistentOverviewBeforeFirstDraw()
    {
        using var a = new G02BHgtFixture("N23E120");
        using var b = new G02BHgtFixture("N23E121");
        using var c = new G02BHgtFixture("N24E121");
        string dir = Path.Combine(AppContext.BaseDirectory, $"XYE-Pyramid-{Guid.NewGuid():N}");
        try
        {
            var catalog = new NasademTileCatalog([a.Path, b.Path, c.Path]);
            var cold = new GeoTerrainRenderSession(catalog, 1, true, dir);
            Assert.Equal(0, cold.OverviewCacheHits);
            var warm = new GeoTerrainRenderSession(catalog, 1, true, dir);
            Assert.Equal(3, warm.OverviewCacheHits);
            Assert.Equal(3 * 188644L, warm.OverviewBytes);
            Assert.Equal(0L, warm.SourceResidency.LoadCount);
            Assert.NotEmpty(warm.Draws(1000, 700));
            Assert.InRange(warm.SourceResidency.ResidentCount, 0, 1);
        }
        finally { Directory.Delete(dir, true); }
    }
}