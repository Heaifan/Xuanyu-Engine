using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_WorkbenchSimulation
{
    [Fact]
    public void PlayStop_ShouldKeepEditorSceneAndRenderBridgeUnchanged()
    {
        var wb = new Workbench(use3D: true);
        var terrain = Assert.Single(wb.Scene.Objects);
        Transform original = wb.World.Find(terrain.Id)!.Transform;
        var before = Assert.Single(wb.Scene.Draws);
        wb.StartSimulation();
        Assert.True(wb.IsSimulating);
        Assert.NotSame(wb.World, wb.Simulation.RunningWorld);
        Assert.Equal(terrain.Id, Assert.Single(
            wb.Simulation.RunningWorld!.Entities).Id);
        for (int i = 0; i < 30; i++)
            wb.AdvanceSimulationTick();
        Assert.Equal(1d, wb.Simulation.ElapsedSeconds);
        wb.Simulation.RunningWorld.SetTransform(terrain.Id, new Transform(
            new Vector3(5, 0, 0), Quaternion.Identity, Vector3.One));
        Assert.Equal(original, wb.World.Find(terrain.Id)!.Transform);
        Assert.Equal(before.ModelToClip, wb.Scene.Draws[0].ModelToClip);
        wb.StopSimulation();
        Assert.False(wb.IsSimulating);
        Assert.Null(wb.Simulation.RunningWorld);
        Assert.Equal(terrain.Id, Assert.Single(wb.Scene.Objects).Id);
        Assert.Contains(wb.Log.Items, line => line.Contains("模拟停止"));
    }
}