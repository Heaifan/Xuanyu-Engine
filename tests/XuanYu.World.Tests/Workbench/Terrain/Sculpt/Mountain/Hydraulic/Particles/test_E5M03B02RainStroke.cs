using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M03B02RainStroke
{
    [Fact]
    public void Stroke_HoldAndDragApplyRainWithoutBridgingMiss()
    {
        var field = test_E5M03B02RainStamp.CreateSlope(1f, 1f);
        var before = field.Samples.ToArray();
        var brush = new TerrainBrushSettings(8f, 0.5f, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, false, rain: true);
        Assert.True(stroke.MoveTo(new(12, 20)) > 0);
        Assert.True(stroke.Advance(0.1f) > 0);
        stroke.MoveTo(new(30, 20));
        Assert.True(stroke.Advance(0.2f) > 0);
        stroke.BreakPath();
        Assert.Equal(0, stroke.Advance(0.1f));
        Assert.True(stroke.MoveTo(new(65, 20)) > 0);
        Assert.Equal(before[20 * field.SamplesX + 48], field.GetHeight(48, 20));
        var after = field.Samples.ToArray();
        Assert.Contains(after.Zip(before), p => p.First < p.Second);
        Assert.Contains(after.Zip(before), p => p.First > p.Second);
        Assert.InRange(Math.Abs(after.Sum(z => (double)z) - before.Sum(z => (double)z)), 0d, 0.02d);
    }

    [Fact]
    public void Workbench_RainUsesOwnToolAndOneHistoryEntryIncludingRelease()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1); wb.SwitchTo(PageId.Terrain);
        wb.ActiveCamera!.SnapAxis(Vector3.UnitZ);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SetTerrainBrush(new TerrainBrushSettings(15f, 0.5f, 0.5f));
        wb.SelectTerrainMountainTool();
        wb.SetTerrainMountainHeight(30f);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        var field = wb.TerrainMap.HeightField;
        var before = field.Samples.ToArray();

        wb.SelectTerrainRainTool();
        Assert.True(wb.IsTerrainRainTool);
        Assert.False(wb.IsTerrainThermalTool || wb.IsTerrainMountainTool || wb.IsTerrainElevationTool ||
            wb.IsTerrainSmoothTool || wb.IsTerrainFlattenTool);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        var first = field.Samples.ToArray();
        Assert.Contains(first.Zip(before), p => p.First < p.Second);
        wb.MoveTerrainSculpt(new(410, 300), 800, 600);
        wb.EndTerrainSculpt();
        var after = field.Samples.ToArray();
        Assert.Contains(after.Zip(first), p => p.First != p.Second);
        Assert.Contains(after.Zip(before), p => p.First > p.Second);
        wb.UndoTerrain(); Assert.Equal(before, field.Samples.ToArray());
        wb.RedoTerrain(); Assert.Equal(after, field.Samples.ToArray());
        wb.SelectTerrainThermalTool();
        Assert.False(wb.IsTerrainRainTool);
        Assert.True(wb.IsTerrainThermalTool);
    }
}