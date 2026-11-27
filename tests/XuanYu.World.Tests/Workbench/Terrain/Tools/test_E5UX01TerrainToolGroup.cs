using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests;

public sealed class test_E5UX01TerrainToolGroup
{
    [Fact]
    public void Groups_SelectExistingToolsWithoutEditingHeights()
    {
        var wb = new Workbench(use3D: true);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        var field = wb.TerrainMap.HeightField;
        var before = field.Samples.ToArray();
        wb.SelectTerrainToolGroup(TerrainToolGroup.Generation);
        Assert.True(wb.IsTerrainMountainTool);
        Assert.Equal(TerrainToolGroup.Generation, wb.ActiveTerrainToolGroup);
        wb.SelectTerrainToolGroup(TerrainToolGroup.Evolution);
        Assert.True(wb.IsTerrainThermalTool);
        wb.SelectTerrainRainTool();
        Assert.Equal(TerrainToolGroup.Evolution, wb.ActiveTerrainToolGroup);
        wb.SelectTerrainToolGroup(TerrainToolGroup.Evolution);
        Assert.True(wb.IsTerrainRainTool);
        wb.SelectTerrainToolGroup(TerrainToolGroup.Sculpt);
        Assert.Equal(TerrainToolGroup.Sculpt, wb.ActiveTerrainToolGroup);
        Assert.False(wb.IsTerrainRainTool);
        wb.SelectTerrainSmoothTool();
        Assert.Equal(TerrainToolGroup.Sculpt, wb.ActiveTerrainToolGroup);
        Assert.Equal(before, field.Samples.ToArray());
        Assert.False(wb.CanUndoTerrain);
    }

    [Theory]
    [InlineData(TerrainToolGroup.Hydrology)]
    [InlineData(TerrainToolGroup.Surface)]
    [InlineData(TerrainToolGroup.Engineering)]
    [InlineData(TerrainToolGroup.Analysis)]
    public void UnimplementedGroups_CannotReplaceActiveTool(TerrainToolGroup group)
    {
        var wb = new Workbench(use3D: true);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SelectTerrainMountainTool();
        Assert.False(wb.CanSelectTerrainToolGroup(group));
        wb.SelectTerrainToolGroup(group);
        Assert.True(wb.IsTerrainMountainTool);
    }

    [Fact]
    public void Groups_RequireEditableTerrainPage()
    {
        var wb = new Workbench(use3D: true);
        wb.SwitchTo(PageId.Terrain);
        Assert.False(wb.CanSelectTerrainToolGroup(TerrainToolGroup.Generation));
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        Assert.True(wb.CanSelectTerrainToolGroup(TerrainToolGroup.Generation));
        wb.SwitchTo(PageId.World);
        Assert.False(wb.CanSelectTerrainToolGroup(TerrainToolGroup.Generation));
        wb.SelectTerrainToolGroup(TerrainToolGroup.Generation);
        Assert.Equal(TerrainToolGroup.Sculpt, wb.ActiveTerrainToolGroup);
    }
}