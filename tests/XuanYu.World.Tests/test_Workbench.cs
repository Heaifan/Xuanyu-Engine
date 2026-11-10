using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests;

public sealed class test_Workbench
{
    [Fact]
    public void Catalog_ShouldHaveFivePages()
    {
        Assert.Equal(5, PageCatalog.All.Count);
        Assert.Equal(PageId.Project, PageCatalog.All[0].Id);
        Assert.Equal(PageId.World, PageCatalog.All[1].Id);
        Assert.Equal(PageId.Terrain, PageCatalog.All[2].Id);
        Assert.Equal(PageId.Simulation, PageCatalog.All[3].Id);
        Assert.Equal(PageId.Tools, PageCatalog.All[4].Id);
    }

    [Fact]
    public void Workbench_Default_ShouldBeWorldAndKeepViewport()
    {
        var wb = new Workbench();

        Assert.Equal(PageId.World, wb.CurrentId);
        Assert.True(wb.KeepViewportAlive);
        Assert.Single(wb.Log.Items);
    }

    [Theory]
    [InlineData(PageId.Project)]
    [InlineData(PageId.World)]
    [InlineData(PageId.Terrain)]
    [InlineData(PageId.Simulation)]
    [InlineData(PageId.Tools)]
    public void SwitchTo_ValidPage_ShouldUpdateCurrent(PageId id)
    {
        var wb = new Workbench(PageId.Project);

        wb.SwitchTo(id);

        Assert.Equal(id, wb.CurrentId);
        Assert.Equal(id, wb.Current.Id);
        Assert.True(wb.KeepViewportAlive);
    }

    [Fact]
    public void SwitchTo_InvalidPage_ShouldThrow()
    {
        var wb = new Workbench();
        Assert.Throws<ArgumentOutOfRangeException>(() => wb.SwitchTo((PageId)99));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void LogBox_BlankText_ShouldThrow(string blank)
    {
        var log = new LogBox();
        Assert.Throws<ArgumentException>(() => log.Write(blank));
    }
}