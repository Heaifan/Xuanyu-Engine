using Xunit;
using XuanYu.Runtime;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests;

public sealed class test_E602BRuntimeSpatial
{
    [Fact]
    public void PlayStopPreservesAndIsolatesPrecisePosition()
    {
        var editor = new WorldScene();
        var p = SpatialPosition.FromLocal(1_000_000.01, 2, 3);
        var id = editor.Create("A", p).Id;
        var session = new SimulationSession(editor);
        session.Start();
        var runtime = Assert.IsType<WorldScene>(session.RunningWorld);
        Assert.Equal(p, runtime.GetSpatialPosition(id));
        var moved = SpatialPosition.FromLocal(1_000_000.02, 2, 3);
        runtime.SetSpatialPosition(id, moved);
        Assert.Equal(moved, runtime.GetSpatialPosition(id));
        Assert.Equal(p, editor.GetSpatialPosition(id));
        session.Stop();
        session.Start();
        Assert.Equal(p, session.RunningWorld!.GetSpatialPosition(id));
    }
}