using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05DemCoverage
{
    [Fact]
    public void EnclosedMissingTile_IsReported()
    {
        var cells = from lat in Enumerable.Range(23, 3)
                    from lon in Enumerable.Range(120, 3)
                    where (lat, lon) != (24, 121)
                    select (lat, lon);
        var report = TerrainDemCoverageReport.Check(cells);
        Assert.Equal(1, report.MissingCount);
        Assert.Equal("N24E121", Assert.Single(report.Holes));
    }

    [Fact]
    public void TaiwanFootprint_HasUnselectedExteriorRatherThanEnclosedHole()
    {
        var cells = new[] { (21,120), (21,121), (22,120), (22,121),
            (23,119), (23,120), (23,121), (24,120), (24,121), (24,122),
            (25,121), (25,122) };
        var report = TerrainDemCoverageReport.Check(cells);
        Assert.Equal(8, report.MissingCount);
        Assert.Empty(report.Holes);
    }
}