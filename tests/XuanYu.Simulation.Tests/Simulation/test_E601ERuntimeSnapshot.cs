using Xunit;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_E601ERuntimeSnapshot
{
    private readonly record struct Custom(int Value);
    private readonly record struct RuntimeOnly(int Value);

    [Fact]
    public void Play_CopiesCustomComponentsAndIsolatesBothDirections()
    {
        var editor = new WorldScene();
        var entity = editor.Create("A");
        editor.AddComponent(entity.Id, new Custom(10));
        var session = new SimulationSession(editor);
        session.Start();
        var runtime = Assert.IsType<WorldScene>(session.RunningWorld);
        Assert.Equal(new Custom(10), runtime.GetComponent<Custom>(entity.Id));
        runtime.SetComponent(entity.Id, new Custom(99));
        Assert.Equal(new Custom(10), editor.GetComponent<Custom>(entity.Id));
        editor.SetComponent(entity.Id, new Custom(20));
        Assert.Equal(new Custom(99), runtime.GetComponent<Custom>(entity.Id));
        runtime.AddComponent(entity.Id, new RuntimeOnly(8));
        Assert.False(editor.HasComponent<RuntimeOnly>(entity.Id));
        runtime.RemoveComponent<Custom>(entity.Id);
        Assert.True(editor.HasComponent<Custom>(entity.Id));
    }

    [Fact]
    public void StopAndRestart_DiscardsRuntimeStructureAndReadsLatestEditor()
    {
        var editor = new WorldScene();
        var a = editor.Create("A");
        var b = editor.Create("B");
        editor.AddComponent(a.Id, new Custom(10));
        var session = new SimulationSession(editor);
        session.Start();
        var runtime = Assert.IsType<WorldScene>(session.RunningWorld);
        runtime.SetComponent(a.Id, new Custom(99));
        runtime.Delete(b.Id);
        var c = runtime.Create("Runtime only");
        session.Step();
        session.Stop();
        Assert.Null(session.RunningWorld);
        Assert.NotNull(editor.Find(b.Id));
        Assert.Null(editor.Find(c.Id));
        editor.SetComponent(a.Id, new Custom(42));
        session.Start();
        var next = Assert.IsType<WorldScene>(session.RunningWorld);
        Assert.NotSame(runtime, next);
        Assert.Equal(0L, session.TickCount);
        Assert.Equal(new Custom(42), next.GetComponent<Custom>(a.Id));
        Assert.NotNull(next.Find(b.Id));
        Assert.Null(next.Find(c.Id));
    }
}