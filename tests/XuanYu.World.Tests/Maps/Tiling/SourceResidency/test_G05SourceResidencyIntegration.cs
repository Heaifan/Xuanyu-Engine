using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05SourceResidencyIntegration
{
    [Fact]
    public void MultiWorkbench_UsesFileBackedSourceAndLightweightDocument()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        b.SetHeight(1800, 1800, 2345);
        var wb = new Workbench();
        wb.ImportTerrainFiles([a.Path, b.Path]);
        Assert.Null(wb.TerrainSourceTile);
        Assert.Equal(129, wb.TerrainMap.HeightField.SamplesX);
        Assert.Equal(2, wb.TerrainDemEntries.Count);
        Assert.True(wb.TerrainSourceResidency!.Value.IsFileBacked);
        Assert.Equal(16, wb.TerrainSourceResidency!.Value.Capacity);
        Assert.NotEmpty(wb.GeoTerrainDraws(1000, 700));
        wb.SelectTerrainDem("N24E121");
        Assert.Equal((short)2345, wb.SelectedTerrainDemSummary!.Value.MaxElevation);
        Assert.Contains(wb.GeoTerrainDraws(1000, 700), d => d.Selected);
        wb.SelectTerrain(true);
        Assert.Null(wb.SelectedTerrainDemSummary);
        wb.ImportTerrain(a.Path);
        Assert.NotNull(wb.TerrainSourceTile);
        Assert.False(wb.TerrainSourceResidency!.Value.IsFileBacked);
        Assert.Equal(NasademTile.Side, wb.TerrainMap.HeightField.SamplesX);
    }

    [Fact]
    public void RealSession_WithSmallSourceCacheMaintainsCoverage()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        using var c = new G02BHgtFixture("N25E121");
        var session = new GeoTerrainRenderSession(new NasademTileCatalog([a.Path, b.Path, c.Path]), 2);
        Assert.Equal(3, session.SourceCount);
        foreach (var target in new[] { new GeoCoordinate(24.5, 120.5, 0),
            new GeoCoordinate(25.5, 121.5, 0) })
        {
            session.Camera.FrameTop(target, 150000d);
            for (int i = 0; i < 3; i++)
            {
                Assert.NotEmpty(session.Draws(1000, 700));
                Assert.InRange(session.SourceResidency.ResidentCount, 1, 2);
                Assert.InRange(session.SourceResidency.ResidentBytes, 1L, 2 * NasademTile.ByteLength);
            }
        }
        Assert.True(session.SourceResidency.EvictedCount > 0);
    }

    [Fact]
    public void TwelveTileWorkbench_ReusesDecodedSourcesAcrossNavigationAndSelection()
    {
        string[] ids = ["N21E120", "N21E121", "N22E120", "N22E121",
            "N23E119", "N23E120", "N23E121", "N24E120", "N24E121",
            "N24E122", "N25E121", "N25E122"];
        var fixtures = new List<G02BHgtFixture>();
        try
        {
            foreach (string id in ids) fixtures.Add(new G02BHgtFixture(id));
            var wb = new Workbench();
            wb.ImportTerrainFiles(fixtures.Select(f => f.Path).ToArray());
            foreach (string id in ids)
            {
                wb.SelectTerrainDem(id);
                Assert.NotNull(wb.SelectedTerrainDemSummary);
            }
            wb.SelectTerrain(true);
            var warm = wb.TerrainSourceResidency!.Value;
            Assert.Equal(16, warm.Capacity);
            Assert.Equal(12, warm.ResidentCount);
            Assert.Equal(12L, warm.LoadCount);
            Assert.Equal(0L, warm.EvictedCount);
            foreach (var target in new[] { new GeoCoordinate(21.5, 120.5, 0),
                new GeoCoordinate(25.5, 121.5, 0) })
            {
                wb.GeoTerrainCamera!.FrameTop(target, 150_000d);
                for (int i = 0; i < 3; i++)
                    Assert.NotEmpty(wb.GeoTerrainDraws(1000, 700));
            }
            foreach (string id in ids.Reverse()) wb.SelectTerrainDem(id);
            var after = wb.TerrainSourceResidency!.Value;
            Assert.Equal(warm, after);
        }
        finally { foreach (var fixture in fixtures) fixture.Dispose(); }
    }
}