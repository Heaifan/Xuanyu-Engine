using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M02CNaturalRidgeStroke
{
    [Fact]
    public void Stroke_SameStraightPathWithDifferentEventCountsAndTimes()
    {
        var a = new TerrainHeightField(101, 101, 1f);
        var b = new TerrainHeightField(101, 101, 1f);
        var fast = Create(a); var slow = Create(b);
        fast.MoveTo(new(15, 50)); fast.MoveTo(new(85, 50));
        Assert.True(fast.FinishMountain() > 0);
        slow.MoveTo(new(15, 50));
        for (int x = 16; x <= 85; x++)
        {
            slow.MoveTo(new(x, 50)); slow.Advance(0.03f);
        }
        Assert.Equal(0, slow.Advance(2f));
        for (int x = 0; x < a.Samples.Length; x++)
            Assert.InRange(MathF.Abs(a.Samples[x] - b.Samples[x]), 0f, 0.001f);
        for (int x = 15; x <= 85; x++) Assert.InRange(a.GetHeight(x, 50), 0.001f, 30f);
        Assert.Equal(0f, a.GetHeight(50, 60));
    }

    [Fact]
    public void Stroke_BreakDoesNotBridgeAndRetracingDoesNotAccumulate()
    {
        var field = new TerrainHeightField(101, 101, 1f);
        var stroke = Create(field);
        stroke.MoveTo(new(10, 50)); stroke.MoveTo(new(25, 50));
        stroke.BreakPath(); stroke.MoveTo(new(75, 50));
        stroke.MoveTo(new(90, 50)); stroke.Advance(0.03f);
        Assert.Equal(0f, field.GetHeight(50, 50));
        Assert.True(field.GetHeight(20, 50) > 0f);
        Assert.True(field.GetHeight(85, 50) > 0f);
        var before = field.Samples.ToArray();
        stroke.MoveTo(new(75, 50)); stroke.Advance(1f);
        for (int x = 0; x < before.Length; x++)
            Assert.InRange(MathF.Abs(field.Samples[x] - before[x]), 0f, 0.001f);
    }

    [Fact]
    public void Workbench_DragUsesOneHistoryEntryAndNoAutomaticErosion()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1); wb.SwitchTo(PageId.Terrain);
        wb.ActiveCamera!.SnapAxis(Vector3.UnitZ);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SelectTerrainMountainTool();
        wb.SetTerrainMountainHeight(30f);
        wb.SetTerrainBrush(new TerrainBrushSettings(10f, 0.5f, 0.5f));

        var matrix = wb.ActiveCamera!.ViewProjection(800, 600);
        var field = wb.TerrainMap.HeightField;
        var before = field.Samples.ToArray();
        Assert.True(wb.TryBeginTerrainSculpt(Project(-25, matrix), 800, 600, false));
        wb.MoveTerrainSculpt(Project(25, matrix), 800, 600);
        wb.AdvanceTerrainSculpt(0.03f);
        Assert.InRange(field.GetHeight(50, 50), 0.001f, 30f);
        var preview = field.Samples.ToArray();
        wb.EndTerrainSculpt();
        Assert.Equal(preview, field.Samples.ToArray());
        wb.UndoTerrain(); Assert.Equal(before, field.Samples.ToArray());
        Assert.False(wb.CanUndoTerrain);
        wb.RedoTerrain(); Assert.Equal(preview, field.Samples.ToArray());
    }

    private static TerrainSculptStroke Create(TerrainHeightField field) => new(field,
        new TerrainBrushSettings(10f, 0.5f, 0.5f), false,
        naturalPeakHeight: 30f);

    private static Vector2 Project(float x, Matrix4x4 matrix)
    {
        var clip = Vector4.Transform(new Vector4(x, 0, 0, 1), matrix);
        return new((clip.X / clip.W + 1f) * 400f, (clip.Y / clip.W + 1f) * 300f);
    }
}