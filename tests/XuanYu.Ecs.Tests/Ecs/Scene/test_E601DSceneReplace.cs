using System.Numerics;
using Xunit;
using XuanYu.World;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601DSceneReplace
{
    [Fact]
    public void Replace_RestoresAllValuesAndRemovesOldEntities()
    {
        var scene = new WorldScene();
        var old = scene.Create("Old");
        var a = new WorldEntity(Guid.NewGuid(), "A", false,
            Transform.Identity with { Position = new Vector3(1, 2, 3) });
        var b = new WorldEntity(Guid.NewGuid(), "B", true, Transform.Identity);
        scene.Replace([b, a]);
        Assert.Null(scene.Find(old.Id));
        Assert.Equal(new[] { b, a }, scene.Entities.ToArray());
        Assert.Equal(a, scene.Find(a.Id));
        scene.SetVisible(a.Id, true);
        Assert.False(a.IsVisible);
        Assert.Equal(b, scene.Find(b.Id));
        scene.Replace([]);
        Assert.Empty(scene.Entities);
        Assert.Null(scene.Find(a.Id));
    }

    [Fact]
    public void Replace_DuplicateGuidLeavesExistingSceneUntouched()
    {
        var scene = new WorldScene();
        var old = scene.Create("Old");
        var item = new WorldEntity(Guid.NewGuid(), "New", true, Transform.Identity);
        Assert.Throws<ArgumentException>(() => scene.Replace([item, item]));
        Assert.Equal(old, Assert.Single(scene.Entities));
        Assert.Null(scene.Find(item.Id));
    }

    [Fact]
    public void Replace_EmptyGuidLeavesExistingSceneUntouched()
    {
        var scene = new WorldScene();
        var old = scene.Create("Old");
        var invalid = new WorldEntity(Guid.Empty, "Invalid", true, Transform.Identity);
        Assert.Throws<ArgumentException>(() => scene.Replace([invalid]));
        Assert.Equal(old, Assert.Single(scene.Entities));
    }
}