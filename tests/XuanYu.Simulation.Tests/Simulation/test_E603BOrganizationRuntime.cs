using Xunit;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_E603BOrganizationRuntime
{
    [Fact]
    public void PlayStop_IsolatesOrganizationState()
    {
        var editor = new WorldScene();
        var a = editor.Create("A").Id;
        var b = editor.Create("B").Id;
        var c = editor.Create("C").Id;
        editor.SetOrganizationParent(b, a);
        var simulation = new SimulationSession(editor);
        simulation.Start();
        var runtime = simulation.RunningWorld!;
        Assert.Equal(a, runtime.GetOrganizationParent(b));
        runtime.SetOrganizationParent(b, c);
        simulation.Step();
        Assert.Equal(c, runtime.GetOrganizationParent(b));
        Assert.Equal(a, editor.GetOrganizationParent(b));
        simulation.Stop();
        Assert.Null(simulation.RunningWorld);
        simulation.Start();
        Assert.Equal(a, simulation.RunningWorld!.GetOrganizationParent(b));
        simulation.Stop();
    }
}