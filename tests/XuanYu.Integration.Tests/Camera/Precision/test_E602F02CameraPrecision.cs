using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E602F02CameraPrecision
{
    [Fact]
    public void LocalCameraRetainsSubFloatDisplacement()
    {
        var camera = new ViewportCamera();
        camera.Focus(6_000_000d, 6_000_000d, 100d);
        camera.SnapAxis(Vector3.UnitX);
        var before = camera.TargetMeters;
        camera.MoveGround(.01d, .02d);
        Assert.InRange(Math.Abs(camera.TargetMeters.X - before.X + .01d), 0d, 1e-8d);
        Assert.InRange(Math.Abs(camera.TargetMeters.Y - before.Y - .02d), 0d, 1e-8d);
        Assert.Equal(before.Z, camera.TargetMeters.Z);
        Assert.Equal((float)before.X, camera.Target.X);
    }

    [Fact]
    public void PanAndZoomRetainSmallChangesAtLargeLocalOffset()
    {
        var camera = new ViewportCamera();
        camera.Focus(6_000_000d, 6_000_000d, 100d);
        camera.SnapAxis(Vector3.UnitX);
        var before = camera.TargetMeters;
        var distance = camera.DistanceMeters;
        camera.Pan(.01d, 0d, 600);
        Assert.InRange(before.Y - camera.TargetMeters.Y, .00001d, .001d);
        Assert.Equal(before.X, camera.TargetMeters.X);
        camera.Zoom(.0000001d);
        Assert.True(camera.DistanceMeters < distance);
        Assert.Equal(distance * Math.Pow(.85d, .0000001d), camera.DistanceMeters, 12);
    }

    [Fact]
    public void TopViewRetainsGroundHeadingAndValidProjection()
    {
        var camera = new ViewportCamera();
        camera.SnapAxis(Vector3.UnitZ);
        camera.MoveGround(1d, 0d);
        Assert.Equal(1d, camera.TargetMeters.Y, 12);
        Assert.Equal(0d, camera.TargetMeters.Z);
        Assert.True(ViewportScreenRay.TryCreate(camera.ViewProjection(800, 600),
            new Vector2(400, 300), 800, 600, out _, out var direction));
        Assert.True(Vector3.Dot(direction, -Vector3.UnitZ) > .9999f);
    }

    [Fact]
    public void PlayUsesIndependentDoubleCameraAndStopRestoresEditor()
    {
        var wb = new Workbench(use3D: true);
        wb.SwitchTo(PageId.World);
        var editor = wb.Scene.Camera!;
        editor.Focus(6_000_000.01d, 6_000_000.02d, 100d);
        editor.SnapAxis(Vector3.UnitX);
        var original = editor.TargetMeters;
        wb.StartSimulation();
        var runtime = wb.ActiveCamera!;
        Assert.NotSame(editor, runtime);
        Assert.Equal(original, runtime.TargetMeters);
        wb.SetGameCameraKey(GameCameraKey.Forward, true);
        wb.AdvanceSimulationTick();
        Assert.InRange(Math.Abs(runtime.TargetMeters.X - original.X + 4d / 30d), 0d, 1e-8d);
        Assert.Equal(original, editor.TargetMeters);
        wb.StopSimulation();
        Assert.Same(editor, wb.ActiveCamera);
        Assert.Equal(original, editor.TargetMeters);
    }
}