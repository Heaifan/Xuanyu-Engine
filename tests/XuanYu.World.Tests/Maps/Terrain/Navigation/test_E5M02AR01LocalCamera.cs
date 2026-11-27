using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E5M02AR01LocalCamera
{
    [Fact]
    public void SideProjection_IncludesSlopeBehindLogicalEyeAndPicksIt()
    {
        var field = new TerrainHeightField(3, 3, 50f);
        for (int y = 0; y < 3; y++) field.SetHeight(2, y, 40f);
        var preview = new TerrainPreviewSession(new MapDocument("坡面", field), centered: true);
        var camera = preview.Scene.Camera!;
        camera.Frame(new Vector3(0f, 0f, 20f), 5f, 1.4f);
        camera.SnapAxis(Vector3.UnitX);
        Vector3 eye = camera.Position;
        Assert.True(eye.X < 25f);
        var matrix = camera.ViewProjection(800, 600);
        var clip = Vector4.Transform(new Vector4(25f, 0f, 20f, 1f), matrix);
        Assert.InRange(clip.Z / clip.W, 0f, 1f);
        Assert.InRange(clip.X / clip.W, -0.001f, 0.001f);
        Assert.InRange(clip.Y / clip.W, -0.001f, 0.001f);
        Assert.Equal(eye, camera.Position);
        Assert.True(preview.TryPick(new Vector2(400, 300), 800, 600, out var hit));
        Assert.InRange(hit.WorldPosition.X, 74.99f, 75.01f);
        Assert.InRange(hit.WorldPosition.Z, 19.99f, 20.01f);
    }

    [Fact]
    public void SculptAndRedo_RestoreClearanceAboveRaisedTerrain()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100f, 100f, 1f);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SetTerrainBrush(new TerrainBrushSettings(10f, 1f, 0.5f));
        wb.SelectTerrainElevationTool(true);
        wb.SetTerrainElevation(40f, true);
        var camera = wb.ActiveCamera!;
        camera.SnapAxis(Vector3.UnitZ);
        camera.Frame(Vector3.Zero, 5f, 1.4f);
        Assert.True(camera.Position.Z < 40f);
        Assert.True(wb.TryBeginTerrainSculpt(new Vector2(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        Assert.Equal(40f, wb.TerrainMap.HeightField.GetHeight(50, 50));
        Assert.True(camera.Position.Z >= 40.499f);
        wb.UndoTerrain();
        Assert.Equal(0f, wb.TerrainMap.HeightField.GetHeight(50, 50));
        camera.Frame(Vector3.Zero, 5f, 1.4f);
        Assert.True(camera.Position.Z < 40f);
        wb.RedoTerrain();
        Assert.Equal(40f, wb.TerrainMap.HeightField.GetHeight(50, 50));
        Assert.True(camera.Position.Z >= 40.499f);
    }
}