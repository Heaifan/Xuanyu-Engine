using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602F01GeoRuntime
{
    [Fact]
    public void GlobalCameraCopyPreservesDoubleMovementAndSeparatesState()
    {
        var editor = new GlobalCamera(new(23.5d, 120.5d, 100d), 1000d);
        var frame = new EnuFrame(editor.Target);
        var camera = editor.Copy();
        camera.MoveGround(.01d, .02d);
        var delta = frame.ToEnu(camera.TargetEcef);
        Assert.InRange(delta.East, .019999d, .020001d);
        Assert.InRange(delta.North, .009999d, .010001d);
        Assert.Equal(100d, camera.Target.HeightMeters);
        Assert.Equal(1000d, camera.DistanceMeters);
        Assert.Equal(frame.Origin, editor.Target);
        camera.Zoom(1d);
        Assert.Equal(1000d, editor.DistanceMeters);
    }

    [Fact]
    public async Task PlayDrawsRuntimeEntityAndStopRestoresEditorCameraAndWorld()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        source.ConfigureG07Geoid(Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!));
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        wb.SwitchTo(PageId.World);
        source.Camera.FrameTop(new(23.5, 120.5, 0), 100_000d);
        Assert.Equal(GeoSampleStatus.Valid, wb.CreateG07InspectionCube(cpu, out var created).Status);
        var cube = Assert.IsType<PreviewObject>(created);
        var original = wb.World.GetSpatialPosition(cube.Id);
        var editor = source.Camera;
        var target = editor.Target;
        var eye = editor.PositionEcef;
        wb.SetSkyEnabled(false);
        wb.SetWorldGridEnabled(false);
        var frame = source.G07RenderFrame(800, 600);
        var before = Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600, frame));
        wb.StartSimulation();
        Assert.Same(source, wb.G07PreviewSource);
        Assert.NotSame(editor, source.Camera);
        Assert.Equal(eye, source.Camera.PositionEcef);
        var running = wb.Simulation.RunningWorld!;
        Assert.Equal(original, running.GetSpatialPosition(cube.Id));
        var moved = new EnuFrame(original.ToGeo()).ToEcef(new(100, 0, 0));
        running.SetSpatialPosition(cube.Id, SpatialPosition.FromEcef(moved));
        var changed = Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600, frame));
        Assert.NotEqual(before.ModelToClip, changed.ModelToClip);
        Assert.False(changed.Selected);
        Assert.Equal(original, wb.World.GetSpatialPosition(cube.Id));
        wb.SwitchTo(PageId.Simulation);
        wb.SetGameCameraKey(GameCameraKey.Forward, true);
        wb.AdvanceSimulationTick();
        wb.SetGameCameraKey(GameCameraKey.Forward, false);
        Assert.NotEqual(target, source.Camera.Target);
        wb.NavigateGeoTerrain(12, -4, 0, false, 600);
        wb.NavigateGeoTerrain(0, 0, 1, false, 600);
        Assert.Equal(eye, editor.PositionEcef);
        wb.ResetGeoTerrainCamera();
        Assert.NotSame(editor, source.Camera);
        wb.StopSimulation();
        Assert.Same(editor, source.Camera);
        Assert.Equal(eye, source.Camera.PositionEcef);
        Assert.Equal(original, wb.World.GetSpatialPosition(cube.Id));
        Assert.Equal(before.ModelToClip, Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600, frame)).ModelToClip);
    }
}