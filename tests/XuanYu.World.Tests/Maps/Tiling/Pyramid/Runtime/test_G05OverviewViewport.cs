using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05OverviewViewport
{
    [Fact]
    public void FarViewport_LargerThanRawCapacityDoesNotThrash()
    {
        using var a = new G02BHgtFixture("N23E120");
        using var b = new G02BHgtFixture("N23E121");
        using var c = new G02BHgtFixture("N24E121");
        var catalog = new NasademTileCatalog([a.Path, b.Path, c.Path]);
        var session = new GeoTerrainRenderSession(catalog, 1, useOverview: true);
        Assert.Equal(3, session.OverviewCount);
        Assert.Equal(3 * 188644L, session.OverviewBytes);
        Assert.Equal(0L, session.SourceResidency.LoadCount);
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 150_000d);
        for (int i = 0; i < 32; i++)
        {
            Assert.NotEmpty(session.Draws(1000, 700));
            Assert.InRange(session.SourceResidency.ResidentCount, 0, 1);
        }
        Assert.Equal(1L, session.SourceResidency.LoadCount);
        Assert.Equal(0L, session.SourceResidency.EvictedCount);
        Assert.Equal(0, session.NativeDrawCount);
    }
}