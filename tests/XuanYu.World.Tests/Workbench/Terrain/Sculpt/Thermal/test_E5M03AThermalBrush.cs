using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M03AThermalBrush
{
    [Fact]
    public void Stroke_HoldAndDragRedistributeExistingReliefLocally()
    {
        var field = new TerrainHeightField(25, 15, 1f);
        field.SetHeight(5, 7, 30f);
        field.SetHeight(19, 7, -20f);
        var before = field.Samples.ToArray().Sum(h => (double)h);
        var stroke = new TerrainSculptStroke(field,
            new TerrainBrushSettings(4f, 1f, 0.5f), false, thermal: true);
        Assert.True(stroke.MoveTo(new(5, 7)) > 0);
        float first = field.GetHeight(5, 7);
        Assert.InRange(first, 0f, 29.99f);
        Assert.True(stroke.Advance(0.1f) > 0);
        Assert.True(field.GetHeight(5, 7) < first);
        Assert.Equal(0, stroke.MoveTo(new(19, 7)));
        Assert.True(stroke.Advance(0.1f) > 0);
        Assert.True(field.GetHeight(19, 7) > -20f);
        Assert.Equal(0f, field.GetHeight(19, 0));
        stroke.BreakPath();
        Assert.Equal(0, stroke.Advance(0.1f));
        Assert.InRange(Math.Abs(field.Samples.ToArray().Sum(h => (double)h) - before), 0d, 0.001d);
    }

    [Fact]
    public void Stroke_ZeroStrengthAndGentleSlopeDoNotChangeHeight()
    {
        var field = new TerrainHeightField(21, 21, 1f);
        for (int y = 0; y < 21; y++)
        for (int x = 0; x < 21; x++) field.SetHeight(x, y, x * 0.2f - 5f);
        var before = field.Samples.ToArray();
        var gentle = new TerrainSculptStroke(field,
            new TerrainBrushSettings(6f, 1f, 0.5f), false, thermal: true);
        Assert.Equal(0, gentle.MoveTo(new(10, 10)));
        Assert.Equal(0, gentle.Advance(0.1f));
        Assert.Equal(before, field.Samples.ToArray());
        field.SetHeight(10, 10, 40f);
        before = field.Samples.ToArray();
        var zero = new TerrainSculptStroke(field,
            new TerrainBrushSettings(6f, 0f, 0.5f), true, thermal: true);
        Assert.Equal(0, zero.MoveTo(new(10, 10)));
        Assert.Equal(0, zero.Advance(0.1f));
        Assert.Equal(before, field.Samples.ToArray());
    }

    [Fact]
    public void Workbench_ThermalIsIndependentAndUndoRestoresOneStroke()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1); wb.SwitchTo(PageId.Terrain);
        wb.ActiveCamera!.SnapAxis(Vector3.UnitZ);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SetTerrainBrush(new TerrainBrushSettings(8f, 1f, 0.5f));
        wb.SelectTerrainElevationTool(true); wb.SetTerrainElevation(30f, false);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        var field = wb.TerrainMap.HeightField;
        var before = field.Samples.ToArray();
        wb.SelectTerrainThermalTool();
        Assert.True(wb.IsTerrainThermalTool);
        Assert.False(wb.IsTerrainElevationTool);
        Assert.False(wb.IsTerrainMountainTool);
        Assert.False(wb.IsTerrainSmoothTool);

        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.AdvanceTerrainSculpt(0.1f); wb.EndTerrainSculpt();
        Assert.True(field.GetHeight(50, 50) < 30f);
        var after = field.Samples.ToArray();
        wb.UndoTerrain(); Assert.Equal(before, field.Samples.ToArray());
        wb.RedoTerrain(); Assert.Equal(after, field.Samples.ToArray());
        wb.SelectTerrainMountainTool();
        Assert.False(wb.IsTerrainThermalTool);
        Assert.True(wb.IsTerrainMountainTool);
    }
}