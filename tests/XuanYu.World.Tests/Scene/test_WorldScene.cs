using System.Numerics;
using Xunit;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_WorldScene
{
    [Fact]
    public void Create_ShouldAssignUniqueIdsAndDefaults()
    {
        var scene = new WorldScene();
        var a = scene.Create(" Cube_001 ");
        var b = scene.Create("Cube_002");

        Assert.NotEqual(a.Id, b.Id);
        Assert.Equal("Cube_001", a.Name);
        Assert.True(a.IsVisible);
        Assert.Equal(Transform.Identity, a.Transform);
        Assert.Equal(a, scene.Find(a.Id));
        Assert.Equal(2, scene.Entities.Count);
    }

    [Fact]
    public void SetTransform_ShouldOnlyUpdateTarget()
    {
        var scene = new WorldScene();
        var a = scene.Create("Cube_001");
        var b = scene.Create("Cube_002");

        var transform = new Transform(
            new Vector3(2.5f, 0f, 1f),
            Quaternion.CreateFromYawPitchRoll(0.5f, 0f, 0f),
            new Vector3(2f, 1f, 1f));

        var updated = scene.SetTransform(a.Id, transform);

        Assert.Equal(a.Id, updated.Id);
        Assert.Equal(transform, updated.Transform);
        Assert.Equal(transform, scene.Find(a.Id)!.Transform);
        Assert.Equal(Transform.Identity, a.Transform);
        Assert.Equal(Transform.Identity, scene.Find(b.Id)!.Transform);
    }

    [Fact]
    public void SetVisible_ShouldOnlyUpdateTarget()
    {
        var scene = new WorldScene();
        var a = scene.Create("A");
        var b = scene.Create("B");

        scene.SetVisible(a.Id, false);

        Assert.False(scene.Find(a.Id)!.IsVisible);
        Assert.True(scene.Find(b.Id)!.IsVisible);
        Assert.True(a.IsVisible);
    }

    [Fact]
    public void Delete_ShouldRemoveOnlyTarget()
    {
        var scene = new WorldScene();
        var a = scene.Create("A");
        var b = scene.Create("B");
        var snapshot = scene.Entities;

        Assert.True(scene.Delete(a.Id));
        Assert.False(scene.Delete(a.Id));
        Assert.Null(scene.Find(a.Id));
        Assert.NotNull(scene.Find(b.Id));
        Assert.Single(scene.Entities);
        Assert.Equal(2, snapshot.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_ShouldThrow(string name)
    {
        var scene = new WorldScene();

        Assert.Throws<ArgumentException>(() => scene.Create(name));
    }

    [Fact]
    public void Update_MissingEntity_ShouldThrow()
    {
        var scene = new WorldScene();
        var id = Guid.NewGuid();

        Assert.Throws<KeyNotFoundException>(
            () => scene.SetTransform(id, Transform.Identity));
        Assert.Throws<KeyNotFoundException>(
            () => scene.SetVisible(id, false));
    }
}