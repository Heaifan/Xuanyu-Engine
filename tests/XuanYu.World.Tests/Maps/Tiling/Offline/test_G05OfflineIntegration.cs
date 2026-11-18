using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05OfflineIntegration
{
    [Fact]
    public void OfflinePacks_LoadIntoExistingCatalogAndViewportWithoutSources()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        a.SetHeight(1800, 1800, 1234);
        b.SetHeight(1800, 1800, 2345);
        string dir = Path.Combine(AppContext.BaseDirectory,
            $"XYE-G05D04-Off-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            string x = Path.Combine(dir, "N24E120.xyt");
            string y = Path.Combine(dir, "N24E121.xyt");
            XYTerrainWriter.Write(a.Path, x);
            XYTerrainWriter.Write(b.Path, y);
            var catalog = NasademTileCatalog.Scan(dir);
            Assert.Equal(2, catalog.Count);
            Assert.Equal((short)1234, catalog.Load(24, 120)!.HeightAt(1800, 1800));
            Assert.Equal((short)2345, catalog.Load(24, 121)!.HeightAt(1800, 1800));
            var session = new GeoTerrainRenderSession(catalog, cacheCapacity: 4,
                useOverview: true, overviewCacheDirectory: Path.Combine(dir, "cache"),
                streamRaw: true);
            session.Camera.FrameTop(new GeoCoordinate(24.5d, 121d, 0d), 150_000d);
            Assert.NotEmpty(session.Draws(900, 600));
            Assert.Equal(2, session.SourceCount);
            Assert.Equal(0L, session.SourceResidency.LoadCount);
            Assert.Equal(2, session.OverviewCount);
        }
        finally { Directory.Delete(dir, true); }
    }
}