using System.Numerics;
using Xunit;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E505AR05R07
{
    [Theory]
    [InlineData(false, 1f)]
    [InlineData(true, -1f)]
    public void HeldStroke_AccumulatesByTimeNotPointerEvents(bool lower, float sign)
    {
        var field = new TerrainHeightField(41, 21, 1f);
        var brush = new TerrainBrushSettings(10f, 1f, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, lower);
        stroke.MoveTo(new Vector2(20, 10));
        for (int i = 0; i < 20; i++) stroke.MoveTo(new Vector2(20, 10));
        Assert.Equal(sign, field.GetHeight(20, 10));
        stroke.Advance(1f);
        Assert.Equal(sign * 3f, field.GetHeight(20, 10));
        stroke.Advance(2f);
        Assert.Equal(sign * 7f, field.GetHeight(20, 10));
    }

    [Fact]
    public void Stroke_StablePickAndLivePreviewShareXY()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100f, 100f, 1f);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SetTerrainBrush(new TerrainBrushSettings(10f, 1f, 0.5f));
        var pixel = new Vector2(400, 300);
        var before = wb.BuildTerrainBrushPreview(pixel, 800, 600)!;
        Assert.True(wb.TryBeginTerrainSculpt(pixel, 800, 600, false));
        var field = wb.TerrainMap.HeightField;
        var heights = field.Samples.ToArray();
        for (int i = 0; i < 20; i++) wb.MoveTerrainSculpt(pixel, 800, 600);
        Assert.Equal(heights, field.Samples.ToArray());
        var after = wb.BuildTerrainBrushPreview(pixel, 800, 600)!;
        Assert.Equal(before.CenterMeters.X, after.CenterMeters.X);
        Assert.Equal(before.CenterMeters.Y, after.CenterMeters.Y);
        Assert.True(after.CenterMeters.Z > before.CenterMeters.Z);
        Assert.Equal(10f, after.RadiusMeters);
        Assert.Equal(7.5f, after.InnerRadiusMeters);
        Assert.Equal(before.CenterMeters, after.CursorMeters!.Value);
        wb.EndTerrainSculpt();
    }

    [Fact]
    public void Camera_FramesTallTerrainAndCanZoomPastOldLimit()
    {
        var camera = new ViewportCamera();
        camera.SetTerrainCollision(_ => 0f);
        camera.Frame(new Vector3(0f, 0f, 900f), 1000f, 1.4f);
        float initial = Vector3.Distance(camera.Position, camera.Target);
        Assert.True(initial > 500f);
        camera.Zoom(-4f);
        Assert.True(Vector3.Distance(camera.Position, camera.Target) > initial);
        var clip = Vector4.Transform(new Vector4(camera.Target, 1f), camera.ViewProjection(800, 600));
        Assert.InRange(clip.Z / clip.W, 0f, 1f);
        camera.Zoom(-1000f);
        Assert.True(float.IsFinite(camera.Position.Length()));
    }
}