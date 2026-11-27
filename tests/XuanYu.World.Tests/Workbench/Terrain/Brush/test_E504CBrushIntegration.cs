using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E504CBrushIntegration
{
    [Fact]
    public void Preview_UsesMapMetersAndDoesNotChangeHeight()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(1000f, 1000f, 1f);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        var camera = wb.TerrainScene.Camera!;
        camera.Focus(Vector3.Zero);
        camera.SnapAxis(Vector3.UnitZ);
        wb.SetTerrainBrush(new TerrainBrushSettings(20f, 0.7f, 0.5f));
        var field = wb.TerrainMap.HeightField;
        float[] before = field.Samples.ToArray();
        var cursor = new Vector2(400f, 300f);
        var preview = Assert.IsType<TerrainBrushPreview>(
            wb.BuildTerrainBrushPreview(cursor, 800, 600));
        Assert.Equal(0.7f, preview.Strength);
        Assert.NotEmpty(preview.Outer);
        Assert.NotEmpty(preview.Inner);
        AssertRadius(preview.Outer, preview.CenterMeters, 20f);
        AssertRadius(preview.Inner, preview.CenterMeters, 15f);
        Vector3? displayed = wb.PickTerrainPoint(cursor, 800, 600);
        Assert.True(displayed.HasValue);
        Assert.InRange(Vector3.Distance(displayed.GetValueOrDefault(),
            Vector3.Transform(preview.CenterMeters, preview.MapToScene)), 0f, 0.001f);
        Assert.Equal(before, field.Samples.ToArray());
    }

    [Fact]
    public void Preview_RequiresTerrainEditModeAndValidHit()
    {
        var wb = new Workbench(use3D: true);
        var cursor = new Vector2(400f, 300f);
        wb.SwitchTo(PageId.Terrain);
        Assert.Null(wb.BuildTerrainBrushPreview(cursor, 800, 600));
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        Assert.Null(wb.BuildTerrainBrushPreview(new Vector2(-1, 0), 800, 600));
        wb.SwitchTo(PageId.World);
        Assert.Null(wb.BuildTerrainBrushPreview(cursor, 800, 600));
    }

    [Fact]
    public void Surface_UsesExistingTrianglePickingAndRejectsOffMap()
    {
        var field = new TerrainHeightField(2, 2, 2f);
        field.SetHeight(1, 1, 8f);
        float? z = TerrainBrushSurface.Sample(field, new Vector2(1.5f, 0.5f));
        Assert.True(z.HasValue);
        Assert.InRange(z.GetValueOrDefault(), 1.999f, 2.001f);
        Assert.Null(TerrainBrushSurface.Sample(field, new Vector2(-1f, 1f)));
    }

    private static void AssertRadius(IReadOnlyList<TerrainBrushOutline.Segment> lines,
        Vector3 center, float radius)
    {
        foreach (var line in lines)
        {
            Assert.InRange(Vector3.Distance(line.Start, center), radius - 0.001f, radius + 0.001f);
            Assert.InRange(Vector3.Distance(line.End, center), radius - 0.001f, radius + 0.001f);
        }
    }
}