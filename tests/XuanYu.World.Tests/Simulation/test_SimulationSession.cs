using System.Numerics;
using Xunit;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_SimulationSession
{
    [Fact]
    public void Start_ShouldCloneWorldWithoutChangingEditorWorld()
    {
        var editor = new WorldScene();
        WorldEntity entity = editor.Create("Cube_001");
        var session = new SimulationSession(editor);

        session.Start();
        WorldScene runtime = Assert.IsType<WorldScene>(session.RunningWorld);
        Assert.NotSame(editor, runtime);
        Assert.Equal(entity, runtime.Find(entity.Id));

        runtime.SetTransform(entity.Id, new Transform(
            new Vector3(5, 0, 0), Quaternion.Identity, Vector3.One));
        Assert.Equal(Transform.Identity, editor.Find(entity.Id)!.Transform);
        editor.SetVisible(entity.Id, false);
        Assert.True(runtime.Find(entity.Id)!.IsVisible);
    }

    [Fact]
    public void StartStop_ShouldResetAndCountFixedTicks()
    {
        var editor = new WorldScene();
        var entity = editor.Create("Cube_001");
        var session = new SimulationSession(editor);
        Assert.Throws<InvalidOperationException>(() => session.Step());

        session.Start();
        for (int i = 0; i < 30; i++)
            session.Step();
        Assert.Equal(30L, session.TickCount);
        Assert.Equal(1d, session.ElapsedSeconds);
        Assert.Throws<InvalidOperationException>(() => session.Start());
        session.Stop();
        Assert.Null(session.RunningWorld);
        Assert.Equal(Transform.Identity, editor.Find(entity.Id)!.Transform);

        editor.Rename(entity.Id, "NewCube");
        session.Start();
        Assert.Equal(0L, session.TickCount);
        Assert.Equal("NewCube", session.RunningWorld!.Find(entity.Id)!.Name);
        session.Stop();
    }
}