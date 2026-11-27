using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M01AMountainWorkbench
{
    [Fact]
    public void Mountain_UsesExistingHistoryAndExclusiveToolSelection()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        wb.ActiveCamera!.SnapAxis(Vector3.UnitZ);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SetTerrainBrush(new TerrainBrushSettings(8f, 0.5f, 0.5f));
        wb.SelectTerrainMountainTool();

        Assert.True(wb.IsTerrainMountainTool);
        Assert.False(wb.IsTerrainSmoothTool);
        Assert.False(wb.IsTerrainFlattenTool);
        Assert.False(wb.IsTerrainElevationTool);
        wb.SetTerrainMountainHeight(30f);

        var field = wb.TerrainMap.HeightField;
        var before = field.Samples.ToArray();
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        Assert.InRange(field.GetHeight(50, 50), 29.9f, 30.1f);
        var after = field.Samples.ToArray();
        Assert.True(wb.CanUndoTerrain);
        wb.UndoTerrain(); Assert.Equal(before, field.Samples.ToArray());
        wb.RedoTerrain(); Assert.Equal(after, field.Samples.ToArray());
        wb.SelectTerrainFlattenTool();
        Assert.False(wb.IsTerrainMountainTool);
        Assert.True(wb.IsTerrainFlattenTool);
        Assert.Equal(30f, wb.TerrainMountainHeight);
        Assert.Equal(0.5f, wb.TerrainBrush.Strength);
    }
}