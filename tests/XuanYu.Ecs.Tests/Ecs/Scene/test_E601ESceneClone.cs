using System.Numerics;
using Xunit;
using XuanYu.World;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601ESceneClone
{
    private readonly record struct Custom(int Value);

    [Fact]
    public void Clone_PreservesLegacyAndExtensionStateIndependently()
    {
        var source = new WorldScene();
        var a = source.Create("A");
        var b = source.Create("B");
        source.SetVisible(a.Id, false);
        var transform = Transform.Identity with { Position = new Vector3(1, 2, 3) };
        source.SetTransform(b.Id, transform);
        source.AddComponent(a.Id, new Custom(17));
        var clone = source.Clone();
        Assert.Equal(source.Entities.ToArray(), clone.Entities.ToArray());
        Assert.Equal(new Custom(17), clone.GetComponent<Custom>(a.Id));
        clone.SetComponent(a.Id, new Custom(99));
        clone.Rename(a.Id, "Runtime");
        clone.SetVisible(a.Id, true);
        source.SetTransform(b.Id, Transform.Identity);
        Assert.Equal(transform, clone.Find(b.Id)!.Transform);
        Assert.Equal(new Custom(17), source.GetComponent<Custom>(a.Id));
        Assert.Equal("A", source.Find(a.Id)!.Name);
        Assert.False(source.Find(a.Id)!.IsVisible);
        Assert.True(clone.Delete(b.Id));
        Assert.NotNull(source.Find(b.Id));
        var added = clone.Create("Runtime only");
        Assert.Null(source.Find(added.Id));
    }

    [Fact]
    public void Components_RespectSceneLifecycleAndRequiredTransform()
    {
        var scene = new WorldScene();
        var id = scene.Create("A").Id;
        Assert.True(scene.HasComponent<Transform>(id));
        Assert.False(scene.AddComponent(id, Transform.Identity));
        Assert.Throws<InvalidOperationException>(() => scene.RemoveComponent<Transform>(id));
        Assert.True(scene.AddComponent(id, new Custom(4)));
        Assert.False(scene.AddComponent(id, new Custom(5)));
        Assert.Equal(new Custom(4), scene.GetComponent<Custom>(id));
        Assert.True(scene.RemoveComponent<Custom>(id));
        Assert.False(scene.HasComponent<Custom>(id));
        Assert.False(scene.RemoveComponent<Custom>(id));
        Assert.Throws<KeyNotFoundException>(() => scene.GetComponent<Custom>(id));
        Assert.True(scene.Delete(id));
        Assert.Throws<KeyNotFoundException>(() => scene.HasComponent<Custom>(id));
    }

    [Fact]
    public void Replace_RemainsLegacyDtoImport_NotFullSnapshot()
    {
        var source = new WorldScene();
        var a = source.Create("A");
        source.AddComponent(a.Id, new Custom(6));
        var clone = source.Clone();
        clone.Replace(source.Entities.ToArray());
        Assert.False(clone.HasComponent<Custom>(a.Id));
        Assert.Equal(new Custom(6), source.GetComponent<Custom>(a.Id));
        Assert.Equal(a, clone.Find(a.Id));
    }
}