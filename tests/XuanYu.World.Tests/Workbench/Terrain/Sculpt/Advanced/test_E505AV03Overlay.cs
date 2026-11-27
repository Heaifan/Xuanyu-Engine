using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests;

public sealed class test_E505AV03Overlay
{
    [Fact]
    public void LocalHud_UsesMapMetersAndCameraScale()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(1000, 500, 5);
        wb.SwitchTo(PageId.Terrain);
        var camera = wb.ActiveCamera!;
        camera.SnapAxis(Vector3.UnitZ);
        var hit = wb.PickLocalTerrainHud(new(400, 300), 800, 600);
        Assert.NotNull(hit);
        Assert.InRange(hit.Value.X, 499.9f, 500.1f);
        Assert.InRange(hit.Value.Y, 249.9f, 250.1f);
        Assert.Equal(0f, hit.Value.Z, 3);
        double first = wb.TerrainHudMetersPerPixel(800, 600)!.Value;
        double second = wb.TerrainHudMetersPerPixel(1600, 1200)!.Value;
        Assert.True(first > 0d);
        Assert.Equal(first / 2d, second, 8);
        var pose = wb.TerrainMiniMapCamera()!.Value;
        Assert.InRange(pose.East, 499.9d, 500.1d);
        Assert.InRange(pose.North, 249.9d, 250.1d);
    }

    [Fact]
    public void LocalMiniMap_TracksSculptAndUndoWithoutResamplingStationaryMesh()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        var before = wb.TerrainMiniMap()!;
        Assert.True(before.IsLocal);
        Assert.Equal(100d, before.East);
        Assert.Equal(100d, before.North);
        Assert.All(before.Cells, cell => Assert.Equal((byte)2, cell));
        Assert.Same(before, wb.TerrainMiniMap());
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SelectTerrainElevationTool(true);
        wb.SetTerrainElevation(500, true);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        var after = wb.TerrainMiniMap()!;
        Assert.NotSame(before, after);
        Assert.Contains(after.Cells, cell => cell > 2);
        wb.UndoTerrain();
        Assert.Equal(before.Cells, wb.TerrainMiniMap()!.Cells);
    }
}