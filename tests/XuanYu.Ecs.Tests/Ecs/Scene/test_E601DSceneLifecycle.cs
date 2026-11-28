using System.Numerics;
using Xunit;
using XuanYu.World;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601DSceneLifecycle
{
    [Fact]
    public void CreateAndUpdate_KeepIdentityAndIndependentValues()
    {
        var scene = new WorldScene();
        var a = scene.Create(" A ");
        var b = scene.Create("B");
        var transform = Transform.Identity with { Position = new Vector3(7, 8, 9) };
        Assert.Equal("A", a.Name);
        Assert.NotEqual(a.Id, b.Id);
        Assert.Equal(a with { Transform = transform }, scene.SetTransform(a.Id, transform));
        Assert.Equal(a with { Transform = transform, IsVisible = false },
            scene.SetVisible(a.Id, false));
        Assert.Equal(b, scene.Find(b.Id));
        Assert.Equal(Transform.Identity, a.Transform);
        Assert.True(a.IsVisible);
    }

    [Fact]
    public void DeleteAndReuse_KeepSurvivorAndNewEntityIndependent()
    {
        var scene = new WorldScene();
        var first = scene.Create("First");
        var middle = scene.Create("Middle");
        var last = scene.Create("Last");
        var moved = scene.SetTransform(last.Id,
            Transform.Identity with { Position = new Vector3(4, 5, 6) });
        moved = scene.SetVisible(last.Id, false);
        Assert.True(scene.Delete(middle.Id));
        var fresh = scene.Create("Fresh");
        Assert.Null(scene.Find(middle.Id));
        Assert.False(scene.Delete(middle.Id));
        Assert.Equal(moved, scene.Find(last.Id));
        Assert.Equal(first, scene.Find(first.Id));
        Assert.Equal(Transform.Identity, fresh.Transform);
        Assert.True(fresh.IsVisible);
        Assert.Equal(3, scene.Entities.Count);
        Assert.Equal(fresh, scene.Find(fresh.Id));
    }

    [Fact]
    public void Entities_AreDetachedSnapshotsAndKeepLegacyEnumeration()
    {
        var scene = new WorldScene();
        var a = scene.Create("A");
        var b = scene.Create("B");
        var before = scene.Entities;
        scene.Rename(a.Id, "Changed");
        scene.SetVisible(b.Id, false);
        var after = scene.Entities.ToArray();
        Assert.Equal(new[] { a, b }, before.ToArray());
        Assert.Equal(a with { Name = "Changed" }, after[0]);
        Assert.Equal(b with { IsVisible = false }, after[1]);
    }

    [Fact]
    public void Rename_PreservesExistingNameRules()
    {
        var scene = new WorldScene();
        var a = scene.Create("A");
        var b = scene.Create("B");
        Assert.Throws<InvalidOperationException>(() => scene.Rename(b.Id, " A "));
        Assert.Equal(b, scene.Find(b.Id));
        Assert.Equal(a, scene.Rename(a.Id, " A "));
        Assert.Throws<ArgumentException>(() => scene.Rename(a.Id, " "));
        Assert.Throws<KeyNotFoundException>(() => scene.Rename(Guid.NewGuid(), "C"));
        Assert.Equal("A", scene.Create("A").Name);
    }
}