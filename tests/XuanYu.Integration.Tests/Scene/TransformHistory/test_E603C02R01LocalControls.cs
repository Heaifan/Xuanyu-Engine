using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C02R01LocalControls
{
    [Fact]
    public void LocalGizmoEditsTransformWithoutMovingOrganizedChild()
    {
        var wb = new Workbench(use3D: true);
        var a = wb.Scene.CreateCube(); var b = wb.Scene.CreateCube();
        wb.Scene.SetOrganizationParent(b.Id, a.Id);
        var before = wb.World.Find(a.Id)!.Transform;
        var child = wb.World.Find(b.Id)!.Transform;
        wb.DragLocalEntity(a.Id, before, new(3, -2, 1));
        var translated = wb.World.Find(a.Id)!.Transform;
        Assert.Equal(before.Position + new Vector3(3, -2, 1), translated.Position);
        wb.DragLocalOrientation(a.Id, translated, GeoGizmoMode.Rotate, new Vector3(0, 0, 90));
        var rotated = wb.World.Find(a.Id)!.Transform;
        Assert.NotEqual(translated.Rotation, rotated.Rotation);
        wb.DragLocalOrientation(a.Id, rotated, GeoGizmoMode.Scale, new Vector3(2));
        Assert.Equal(rotated.Scale * 2, wb.World.Find(a.Id)!.Transform.Scale);
        Assert.Equal(child, wb.World.Find(b.Id)!.Transform);
        Assert.Equal(a.Id, wb.World.GetOrganizationParent(b.Id));
        wb.Scene.UndoTransform(); Assert.Equal(rotated, wb.World.Find(a.Id)!.Transform);
        wb.Scene.UndoTransform(); Assert.Equal(translated, wb.World.Find(a.Id)!.Transform);
        wb.Scene.UndoTransform(); Assert.Equal(before, wb.World.Find(a.Id)!.Transform);
    }

    [Fact]
    public void SimulationPage_WasdMovesRuntimeCameraAndStopRestoresEditor()
    {
        var wb = new Workbench(use3D: true);
        var editor = wb.ActiveCamera!;
        var original = editor.Target;
        wb.SwitchTo(PageId.Simulation);
        wb.StartSimulation();
        Assert.True(wb.CanNavigateRuntimeCamera);
        foreach (var key in new[] { GameCameraKey.Forward, GameCameraKey.Left,
            GameCameraKey.Backward, GameCameraKey.Right })
        {
            var start = wb.ActiveCamera!.Target;
            wb.SetGameCameraKey(key, true);
            wb.AdvanceSimulationTick();
            wb.SetGameCameraKey(key, false);
            Assert.NotEqual(start, wb.ActiveCamera!.Target);
        }
        var released = wb.ActiveCamera!.Target;
        wb.AdvanceSimulationTick();
        Assert.Equal(released, wb.ActiveCamera!.Target);
        Assert.Equal(original, editor.Target);
        wb.StopSimulation();
        Assert.Same(editor, wb.ActiveCamera);
        Assert.Equal(original, wb.ActiveCamera!.Target);
    }
}