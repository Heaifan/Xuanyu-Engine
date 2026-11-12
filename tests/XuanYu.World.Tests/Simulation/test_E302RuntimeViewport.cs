using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_E302RuntimeViewport
{
    [Fact]
    public void Play_ShouldRenderRuntimeTransformAndRestoreEditorDraws()
    {
        var wb = new Workbench(use3D: true);
        var cube = Assert.Single(wb.Scene.Objects);
        wb.Scene.Select(cube.Id);
        var editorDraw = Assert.Single(wb.ViewportDraws(800, 600));

        wb.StartSimulation();
        var runtime = wb.Simulation.RunningWorld!;
        var moved = new Transform(new Vector3(3f, 0f, 0f),
            Quaternion.Identity, Vector3.One);
        runtime.SetTransform(cube.Id, moved);

        var runtimeDraw = Assert.Single(wb.ViewportDraws(800, 600));
        Assert.Equal(moved.ToMatrix() * wb.Scene.ViewProjection(800, 600),
            runtimeDraw.ModelToClip);
        Assert.NotEqual(editorDraw.ModelToClip, runtimeDraw.ModelToClip);
        Assert.False(runtimeDraw.Selected);
        Assert.Equal(3f, Assert.Single(wb.RenderDraws).ModelToClip.M41);
        Assert.Equal(Transform.Identity, wb.World.Find(cube.Id)!.Transform);

        wb.StopSimulation();
        var restored = Assert.Single(wb.ViewportDraws(800, 600));
        Assert.Equal(editorDraw.ModelToClip, restored.ModelToClip);
        Assert.True(restored.Selected);
    }

    [Fact]
    public void Play_ShouldKeepMeshBindingsAndRuntimeVisibilityIsolated()
    {
        var wb = new Workbench(use3D: true);
        var second = wb.Scene.CreateCube();
        wb.StartSimulation();
        wb.Simulation.RunningWorld!.SetVisible(second.Id, false);

        var draws = wb.ViewportDraws(800, 600);
        Assert.Equal(2, draws.Count);
        Assert.All(draws, d => Assert.Equal("builtin/cube", d.Mesh.AssetId));
        Assert.False(draws[1].Visible);
        Assert.True(wb.Scene.ViewportDraws(800, 600)[1].Visible);
        wb.StopSimulation();
        Assert.True(wb.ViewportDraws(800, 600)[1].Visible);
    }

    [Fact]
    public void Restart_ShouldRebindTheCurrentEditorScene()
    {
        var wb = new Workbench(use3D: true);
        wb.StartSimulation();
        wb.StopSimulation();
        var added = wb.Scene.CreateCube();
        wb.StartSimulation();

        Assert.Equal(2, wb.ViewportDraws(800, 600).Count);
        Assert.NotNull(wb.Simulation.RunningWorld!.Find(added.Id));
        wb.StopSimulation();
    }
}