using System.Numerics;
using Xunit;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests;

public sealed class test_E303GameCamera
{
    [Fact]
    public void Play_WasdMovesOnlyRuntimeCamera_ThenStopRestoresView()
    {
        var wb = new Workbench(use3D: true);
        var editor = wb.Scene.Camera!;
        Matrix4x4 before = editor.ViewProjection(800, 600);
        wb.StartSimulation();
        Assert.NotSame(editor, wb.ActiveCamera);
        Assert.Equal(before, wb.ActiveCamera!.ViewProjection(800, 600));
        wb.SetGameCameraKey(GameCameraKey.Forward, true);
        for (int i = 0; i < 30; i++) wb.AdvanceSimulationTick();
        wb.SetGameCameraKey(GameCameraKey.Forward, false);
        Assert.NotEqual(before, wb.ActiveCamera!.ViewProjection(800, 600));
        Assert.Equal(before, editor.ViewProjection(800, 600));
        Assert.NotEqual(wb.Scene.ViewportDraws(800, 600)[0].ModelToClip,
            wb.ViewportDraws(800, 600)[0].ModelToClip);
        wb.StopSimulation();
        Assert.Same(editor, wb.ActiveCamera);
        Assert.Equal(before, wb.ActiveCamera!.ViewProjection(800, 600));
    }

    [Fact]
    public void DiagonalMotion_HasSameSpeedAsSingleDirection()
    {
        var wb = new Workbench(use3D: true);
        wb.StartSimulation();
        Vector3 start = wb.ActiveCamera!.Target;
        wb.SetGameCameraKey(GameCameraKey.Forward, true);
        wb.SetGameCameraKey(GameCameraKey.Right, true);
        wb.AdvanceSimulationTick();
        float distance = Vector3.Distance(start, wb.ActiveCamera!.Target);
        Assert.InRange(distance, 4f / 30f - 0.0001f, 4f / 30f + 0.0001f);
        wb.StopSimulation();
    }

    [Fact]
    public void FocusResetAndPageSwitch_ShouldStopHeldMovement()
    {
        var wb = new Workbench(use3D: true);
        wb.StartSimulation();
        Vector3 start = wb.ActiveCamera!.Target;
        wb.SetGameCameraKey(GameCameraKey.Forward, true);
        wb.SetGameCameraKey(GameCameraKey.None, false);
        wb.AdvanceSimulationTick();
        Assert.Equal(start, wb.ActiveCamera!.Target);
        wb.SetGameCameraKey(GameCameraKey.Right, true);
        wb.SwitchTo(PageId.Terrain);
        wb.AdvanceSimulationTick();
        wb.SwitchTo(PageId.World);
        wb.AdvanceSimulationTick();
        Assert.Equal(start, wb.ActiveCamera!.Target);
        wb.StopSimulation();
    }

    [Fact]
    public void RuntimeMouseOrbit_ShouldNotChangeEditorCamera()
    {
        var wb = new Workbench(use3D: true);
        Matrix4x4 editorView = wb.Scene.Camera!.ViewProjection(800, 600);
        wb.StartSimulation();
        wb.ActiveCamera!.Orbit(70f, 20f);
        wb.ActiveCamera!.Zoom(1f);
        Assert.NotEqual(editorView, wb.ActiveCamera!.ViewProjection(800, 600));
        wb.StopSimulation();
        Assert.Equal(editorView, wb.Scene.Camera!.ViewProjection(800, 600));
    }
}