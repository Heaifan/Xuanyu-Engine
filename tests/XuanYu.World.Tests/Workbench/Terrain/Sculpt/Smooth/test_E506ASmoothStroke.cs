using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E506ASmoothStroke
{
    [Fact]
    public void HoldAndMove_SmoothWithoutRaisingOrLoweringFlatGround()
    {
        var field = new TerrainHeightField(21, 11, 1f, 1f);
        field.SetHeight(3, 5, 20f);
        field.SetHeight(15, 5, -20f);
        var brush = new TerrainBrushSettings(3f, 0.8f, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, true, smooth: true);
        Assert.True(stroke.MoveTo(new(3, 5)) > 0);
        float first = field.GetHeight(3, 5);
        Assert.InRange(first, 0f, 19f);
        Assert.True(stroke.Advance(0.1f) > 0);
        Assert.True(field.GetHeight(3, 5) < first);
        Assert.Equal(0, stroke.MoveTo(new(17, 5)));
        Assert.True(stroke.Advance(0.1f) > 0);
        Assert.True(field.GetHeight(15, 5) > -20f);
        Assert.Equal(0f, field.GetHeight(15, 0));
        stroke.BreakPath();
        Assert.Equal(0, stroke.Advance(0.1f));
    }

    [Fact]
    public void WorkbenchSmooth_UsesExistingHistoryAndExclusiveToolSelection()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        wb.ActiveCamera!.SnapAxis(Vector3.UnitZ);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SetTerrainBrush(new TerrainBrushSettings(8f, 1f, 1f));
        wb.SelectTerrainElevationTool(true);
        wb.SetTerrainElevation(100f, false);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        var field = wb.TerrainMap.HeightField;
        var before = field.Samples.ToArray();
        Assert.Equal(100f, field.GetHeight(50, 50));
        wb.SelectTerrainSmoothTool();
        Assert.True(wb.IsTerrainSmoothTool);
        Assert.False(wb.IsTerrainElevationTool);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.AdvanceTerrainSculpt(0.1f);
        wb.EndTerrainSculpt();
        Assert.True(field.GetHeight(50, 50) < 100f);
        var after = field.Samples.ToArray();
        Assert.True(wb.CanUndoTerrain);
        wb.UndoTerrain();
        Assert.Equal(before, field.Samples.ToArray());
        wb.RedoTerrain();
        Assert.Equal(after, field.Samples.ToArray());
        wb.SelectTerrainElevationTool(false);
        Assert.False(wb.IsTerrainSmoothTool);
        Assert.False(wb.IsTerrainElevationTool);
        wb.SelectTerrainSmoothTool();
        wb.SelectTerrainElevationTool(true);
        Assert.True(wb.IsTerrainElevationTool);
        Assert.False(wb.IsTerrainSmoothTool);
        Assert.Equal(100f, wb.TerrainTargetElevation);
    }
}