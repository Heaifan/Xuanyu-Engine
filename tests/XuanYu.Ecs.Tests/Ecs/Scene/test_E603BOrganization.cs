using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E603BOrganization
{
    [Fact]
    public void Reparent_UpdatesBothDirectionsAndSupportsDetach()
    {
        var world = new WorldScene();
        var a = world.Create("A").Id;
        var b = world.Create("B").Id;
        var c = world.Create("C").Id;
        Assert.False(world.HasOrganizationRelations);
        Assert.True(world.SetOrganizationParent(c, a));
        Assert.False(world.SetOrganizationParent(c, a));
        Assert.Equal(a, world.GetOrganizationParent(c));
        Assert.Equal(c, Assert.Single(world.GetOrganizationChildren(a)));
        Assert.True(world.SetOrganizationParent(c, b));
        Assert.Empty(world.GetOrganizationChildren(a));
        Assert.Equal(b, world.GetOrganizationParent(c));
        Assert.Equal(c, Assert.Single(world.GetOrganizationChildren(b)));
        Assert.True(world.SetOrganizationParent(c, null));
        Assert.Null(world.GetOrganizationParent(c));
        Assert.Empty(world.GetOrganizationChildren(b));
        Assert.False(world.HasOrganizationRelations);
    }

    [Fact]
    public void InvalidRelations_LeaveExistingTreeUntouched()
    {
        var world = new WorldScene();
        var a = world.Create("A").Id;
        var b = world.Create("B").Id;
        var c = world.Create("C").Id;
        world.SetOrganizationParent(b, a);
        world.SetOrganizationParent(c, b);
        Assert.Throws<InvalidOperationException>(() => world.SetOrganizationParent(a, a));
        Assert.Throws<InvalidOperationException>(() => world.SetOrganizationParent(a, c));
        Assert.Throws<KeyNotFoundException>(() => world.SetOrganizationParent(c, Guid.NewGuid()));
        Assert.Throws<KeyNotFoundException>(() => world.SetOrganizationParent(Guid.NewGuid(), a));
        Assert.Null(world.GetOrganizationParent(a));
        Assert.Equal(a, world.GetOrganizationParent(b));
        Assert.Equal(b, world.GetOrganizationParent(c));
        Assert.Equal(b, Assert.Single(world.GetOrganizationChildren(a)));
        Assert.Equal(c, Assert.Single(world.GetOrganizationChildren(b)));
    }

    [Fact]
    public void Relations_DoNotInheritTransformOrVisibility()
    {
        var world = new WorldScene();
        var parent = world.Create("Parent");
        var child = world.Create("Child", SpatialPosition.FromGeo(new GeoCoordinate(23, 121, 40)));
        world.SetOrganizationParent(child.Id, parent.Id);
        world.SetTransform(parent.Id, Transform.Identity with { Position = new Vector3(4, 5, 6) });
        world.SetVisible(parent.Id, false);
        Assert.Equal(child, world.Find(child.Id));
        world.SetOrganizationParent(child.Id, null);
        Assert.Equal(child, world.Find(child.Id));
    }

    [Fact]
    public void Children_AreDetachedSnapshots()
    {
        var world = new WorldScene();
        var parent = world.Create("Parent").Id;
        var child = world.Create("Child").Id;
        world.SetOrganizationParent(child, parent);
        var snapshot = world.GetOrganizationChildren(parent);
        world.SetOrganizationParent(child, null);
        Assert.Equal(child, Assert.Single(snapshot));
        Assert.Empty(world.GetOrganizationChildren(parent));
    }
}