using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M02ANaturalPeak
{
    [Fact]
    public void Peak_UsesRelativeHeightAndDoesNotAccumulateOnHold()
    {
        var field = new TerrainHeightField(81, 81, 1f);
        field.SetHeight(40, 40, -20f);
        var brush = new TerrainBrushSettings(15f, 0f, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, false, naturalPeakHeight: 40f);
        Assert.True(stroke.MoveTo(new(40.2f, 40.1f)) > 0);
        Assert.Equal(20f, field.GetHeight(40, 40));
        Assert.Equal(0f, field.GetHeight(55, 40));
        Assert.InRange(field.GetHeight(47, 40), 0.01f, 39.99f);
        var before = field.Samples.ToArray();
        stroke.MoveTo(new(40, 40));
        Assert.Equal(0, stroke.Advance(1f));
        Assert.Equal(0, stroke.FinishMountain());
        Assert.Equal(before, field.Samples.ToArray());
    }

    [Fact]
    public void Peak_IsSeededAndBoundedWithNonSquareSpacing()
    {
        var a = Create(7); var b = Create(7); var c = Create(8);
        Assert.Equal(a.Samples.ToArray(), b.Samples.ToArray());
        Assert.Contains(a.Samples.ToArray().Zip(c.Samples.ToArray()), p => p.First != p.Second);
        Assert.Equal(30f, a.GetHeight(40, 20));
        Assert.Equal(0f, a.GetHeight(56, 20));
        Assert.Equal(0f, a.GetHeight(40, 28));
        Assert.All(a.Samples.ToArray(), h => Assert.InRange(h, 0f, 30f));
    }

    [Fact]
    public void Workbench_PeakFinishesWithoutErosionAndUsesHistory()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        wb.ActiveCamera!.SnapAxis(Vector3.UnitZ);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SelectTerrainMountainTool();
        wb.SetTerrainMountainHeight(30f);
        wb.SetTerrainBrush(new TerrainBrushSettings(15f, 0.5f, 0.5f));

        var field = wb.TerrainMap.HeightField;
        var before = field.Samples.ToArray();
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        var preview = field.Samples.ToArray();
        wb.EndTerrainSculpt();
        Assert.Equal(30f, field.GetHeight(50, 50));
        Assert.Equal(preview, field.Samples.ToArray());
        Assert.True(wb.CanUndoTerrain);
        wb.UndoTerrain(); Assert.Equal(before, field.Samples.ToArray());
        wb.RedoTerrain(); Assert.Equal(preview, field.Samples.ToArray());
    }

    private static TerrainHeightField Create(int seed)
    {
        var field = new TerrainHeightField(81, 41, 1f, 2f);
        var stroke = new TerrainSculptStroke(field, new TerrainBrushSettings(16f, 1f, 0.5f),
            false, naturalPeakHeight: 30f, naturalPeakSeed: seed);
        stroke.MoveTo(new(40, 40));
        return field;
    }
}