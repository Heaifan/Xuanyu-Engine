using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05MultiImport
{
    [Fact]
    public void RejectsDuplicateAndAcceptsNonAdjacentHgt()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N25E121");
        var first = a.Load();
        Assert.Throws<ArgumentException>(() => new GeoTerrainRenderSession(first, first));
        var session = new GeoTerrainRenderSession(first, b.Load());
        Assert.Equal(2, session.SourceCount);
    }

    [Fact]
    public void Workbench_ImportsPairAndDoesNotSavePartialMap()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        var wb = new Workbench();
        wb.ImportTerrainFiles([a.Path, b.Path]);
        Assert.Equal(PageId.Terrain, wb.CurrentId);
        Assert.Equal("N24E120+N24E121", wb.TerrainMultiSourceLabel);
        Assert.Equal(121d, wb.GeoTerrainCamera!.Target.LongitudeDegrees);
        Assert.NotEmpty(wb.ViewportDraws(1280, 720));
        var summary = Assert.Single(wb.Log.Records, entry => entry.Level == LogLevel.Info &&
            entry.Message.StartsWith("G-05", StringComparison.Ordinal));
        Assert.Contains("已导入 2 张 HGT", summary.Message);
        Assert.Contains("派生缓存命中", summary.Message);
        Assert.Contains("SWB=", summary.Message);
        string path = System.IO.Path.ChangeExtension(a.Path, ".xymap");
        Assert.Throws<InvalidOperationException>(() => wb.SaveTerrainMap(path));
        Assert.False(File.Exists(path));
        wb.ImportTerrain(b.Path);
        Assert.Null(wb.TerrainMultiSourceLabel);
        Assert.Equal(121.5d, wb.GeoTerrainCamera!.Target.LongitudeDegrees);
    }
}