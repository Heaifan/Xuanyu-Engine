using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Render;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_E302RuntimeViewport
{
    [Fact]
    public void Play_ShouldRenderRuntimeTransformAndRestoreEditorDraws()
    {
        var wb = new Workbench(use3D: true);
        var terrain = Assert.Single(wb.Scene.Objects);
        Transform original = wb.World.Find(terrain.Id)!.Transform;
        wb.Scene.Select(terrain.Id);
        var editorDraw = LocalTerrain(wb);
        wb.StartSimulation();
        var runtime = wb.Simulation.RunningWorld!;
        var moved = new Transform(new Vector3(3f, 0f, 0f),
            Quaternion.Identity, Vector3.One);
        runtime.SetTransform(terrain.Id, moved);
        var runtimeDraw = LocalTerrain(wb);
        Assert.Equal(moved.ToMatrix() * wb.ActiveCamera!.ViewProjection(800, 600),
            runtimeDraw.ModelToClip);
        Assert.NotEqual(editorDraw.ModelToClip, runtimeDraw.ModelToClip);
        Assert.False(runtimeDraw.Selected);
        Assert.Equal(3f, Assert.Single(wb.RenderDraws).ModelToClip.M41);
        Assert.Equal(original, wb.World.Find(terrain.Id)!.Transform);
        wb.StopSimulation();
        var restored = LocalTerrain(wb);
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
        Assert.Equal(4, draws.Count);
        Assert.StartsWith("terrain/local/", draws[1].Mesh.AssetId);
        Assert.Equal("builtin/cube", draws[2].Mesh.AssetId);
        Assert.False(draws[2].Visible);
        Assert.True(wb.Scene.ViewportDraws(800, 600)[1].Visible);
        wb.StopSimulation();
        Assert.True(Assert.Single(wb.ViewportDraws(800, 600),
            draw => draw.Mesh.AssetId == "builtin/cube").Visible);
    }

    [Fact]
    public void Restart_ShouldRebindTheCurrentEditorScene()
    {
        var wb = new Workbench(use3D: true);
        wb.StartSimulation();
        wb.StopSimulation();
        var added = wb.Scene.CreateCube();
        wb.StartSimulation();
        Assert.Equal(4, wb.ViewportDraws(800, 600).Count);
        Assert.Single(wb.ViewportDraws(800, 600), d => d.Mesh.AssetId == "builtin/cube");
        Assert.NotNull(wb.Simulation.RunningWorld!.Find(added.Id));
        wb.StopSimulation();
    }

    private static RenderDraw LocalTerrain(Workbench wb) =>
        Assert.Single(wb.ViewportDraws(800, 600), draw =>
            draw.Mesh.AssetId.StartsWith("terrain/local/", StringComparison.Ordinal));
}