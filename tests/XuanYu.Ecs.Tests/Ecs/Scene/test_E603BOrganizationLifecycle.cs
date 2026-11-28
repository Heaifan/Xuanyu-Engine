using Xunit;
using XuanYu.World;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E603BOrganizationLifecycle
{
    [Fact]
    public void Delete_DetachesDirectChildrenWithoutDestroyingSubtree()
    {
        var world = new WorldScene();
        var a = world.Create("A").Id;
        var b = world.Create("B").Id;
        var c = world.Create("C").Id;
        var d = world.Create("D").Id;
        world.SetOrganizationParent(b, a);
        world.SetOrganizationParent(c, b);
        world.SetOrganizationParent(d, c);
        Assert.True(world.Delete(b));
        Assert.Empty(world.GetOrganizationChildren(a));
        Assert.Null(world.GetOrganizationParent(c));
        Assert.Equal(c, world.GetOrganizationParent(d));
        Assert.NotNull(world.Find(c));
        Assert.NotNull(world.Find(d));
        Assert.Throws<KeyNotFoundException>(() => world.GetOrganizationChildren(b));
        Assert.False(world.Delete(b));
        var fresh = world.Create("Fresh").Id;
        Assert.Null(world.GetOrganizationParent(fresh));
        Assert.Empty(world.GetOrganizationChildren(fresh));
        Assert.True(world.Delete(d));
        Assert.Empty(world.GetOrganizationChildren(c));
        Assert.False(world.HasOrganizationRelations);
    }

    [Fact]
    public void Clone_PreservesGuidsAndIsolatesRelationsInBothDirections()
    {
        var editor = new WorldScene();
        var a = editor.Create("A").Id;
        var b = editor.Create("B").Id;
        var c = editor.Create("C").Id;
        editor.SetOrganizationParent(b, a);
        var runtime = editor.Clone();
        Assert.Equal(a, runtime.GetOrganizationParent(b));
        Assert.Equal(b, Assert.Single(runtime.GetOrganizationChildren(a)));
        runtime.SetOrganizationParent(b, c);
        editor.Delete(c);
        Assert.Equal(c, runtime.GetOrganizationParent(b));
        Assert.Equal(b, Assert.Single(runtime.GetOrganizationChildren(c)));
        Assert.Equal(a, editor.GetOrganizationParent(b));
        Assert.Equal(b, Assert.Single(editor.GetOrganizationChildren(a)));
        runtime.Delete(c);
        Assert.Null(runtime.GetOrganizationParent(b));
        Assert.Equal(a, editor.GetOrganizationParent(b));
    }

    [Fact]
    public void Replace_ClearsRelationsOnlyAfterSuccessfulLegacyImport()
    {
        var world = new WorldScene();
        var a = world.Create("A");
        var b = world.Create("B");
        world.SetOrganizationParent(b.Id, a.Id);
        Assert.Throws<ArgumentException>(() => world.Replace([a, a]));
        Assert.Equal(a.Id, world.GetOrganizationParent(b.Id));
        world.Replace([a, b]);
        Assert.False(world.HasOrganizationRelations);
        Assert.Null(world.GetOrganizationParent(b.Id));
        Assert.Empty(world.GetOrganizationChildren(a.Id));
    }
}