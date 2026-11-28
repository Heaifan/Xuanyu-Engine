using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World;
using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C01RepresentationBridge
{
    [Fact]
    public void LocalSwitchPreservesEntityAndOrganization()
    {
        var world = new WorldScene();
        var parent = world.Create("Parent");
        var entity = world.Create("Child", SpatialPosition.FromLocal(2, 3, 4));
        world.SetOrganizationParent(entity.Id, parent.Id);
        var bridge = new WorldRenderBridge(world);
        var model = Mesh("model"); var proxy = Mesh("proxy");
        bridge.Bind(entity.Id, model);
        bridge.BindRepresentation(entity.Id, EntityRepresentationKind.Proxy, proxy);
        var before = Assert.Single(bridge.Snapshot(Matrix4x4.Identity, entity.Id));
        Assert.Same(model, before.Mesh);
        bridge.SetRepresentation(entity.Id, EntityRepresentationKind.Proxy);
        var after = Assert.Single(bridge.Snapshot(Matrix4x4.Identity, entity.Id));
        Assert.Same(proxy, after.Mesh);
        Assert.Equal(before.ModelToClip, after.ModelToClip);
        Assert.True(after.Selected);
        bridge.SetRepresentation(entity.Id, EntityRepresentationKind.None);
        Assert.Empty(bridge.Snapshot(Matrix4x4.Identity));
        Assert.Equal(entity, world.Find(entity.Id));
        Assert.Equal(parent.Id, world.GetOrganizationParent(entity.Id));
        bridge.SetRepresentation(entity.Id, EntityRepresentationKind.Mesh);
        Assert.Same(model, Assert.Single(bridge.Snapshot(Matrix4x4.Identity)).Mesh);
    }

    [Fact]
    public void MissingResourceDoesNotChangeActiveAndUnbindClearsAll()
    {
        var world = new WorldScene(); var entity = world.Create("Object");
        var bridge = new WorldRenderBridge(world);
        Assert.Equal(EntityRepresentationKind.None, bridge.GetRepresentation(entity.Id));
        bridge.SetRepresentation(entity.Id, EntityRepresentationKind.None);
        bridge.Bind(entity.Id, Mesh("model"));
        Assert.Throws<InvalidOperationException>(() => bridge.SetRepresentation(entity.Id, EntityRepresentationKind.Proxy));
        Assert.Equal(EntityRepresentationKind.Mesh, bridge.GetRepresentation(entity.Id));
        bridge.BindRepresentation(entity.Id, EntityRepresentationKind.Proxy, Mesh("proxy"));
        Assert.True(bridge.Unbind(entity.Id));
        Assert.Empty(bridge.Snapshot(Matrix4x4.Identity));
        Assert.Equal(EntityRepresentationKind.None, bridge.GetRepresentation(entity.Id));
        Assert.Throws<InvalidOperationException>(() => bridge.SetRepresentation(entity.Id, EntityRepresentationKind.Proxy));
        Assert.NotNull(world.Find(entity.Id));
    }

    [Fact]
    public void GeographicProxyUsesSameRelativeFrameAndVisibility()
    {
        var anchor = new EnuFrame(new GeoCoordinate(24, 121, 100));
        var frame = new GeoRenderFrame(anchor, anchor.ToEcef(new(0, -20, 10)),
            anchor.OriginEcef, 1, .5f, 5000);
        var world = new WorldScene();
        var entity = world.Create("Global", SpatialPosition.FromEcef(anchor.OriginEcef));
        var bridge = new WorldRenderBridge(world);
        bridge.Bind(entity.Id, Mesh("model"));
        var before = Assert.Single(bridge.SnapshotGeographic(frame));
        var proxy = Mesh("proxy");
        bridge.BindRepresentation(entity.Id, EntityRepresentationKind.Proxy, proxy);
        bridge.SetRepresentation(entity.Id, EntityRepresentationKind.Proxy);
        world.SetVisible(entity.Id, false);
        var after = Assert.Single(bridge.SnapshotGeographic(frame, entity.Id));
        Assert.Same(proxy, after.Mesh);
        Assert.Equal(before.ModelToClip, after.ModelToClip);
        Assert.False(after.Visible); Assert.True(after.Selected);
        Assert.Empty(bridge.Snapshot(Matrix4x4.Identity));
    }

    [Fact]
    public void TwoBridgesCanChooseDifferentRepresentationsForSameEntity()
    {
        var world = new WorldScene(); var entity = world.Create("Object");
        var left = new WorldRenderBridge(world); var right = new WorldRenderBridge(world);
        var model = Mesh("model"); var proxy = Mesh("proxy");
        left.Bind(entity.Id, model); right.Bind(entity.Id, model);
        left.BindRepresentation(entity.Id, EntityRepresentationKind.Proxy, proxy);
        left.SetRepresentation(entity.Id, EntityRepresentationKind.Proxy);
        Assert.Same(proxy, Assert.Single(left.Snapshot(Matrix4x4.Identity)).Mesh);
        Assert.Same(model, Assert.Single(right.Snapshot(Matrix4x4.Identity)).Mesh);
    }

    private static RenderMesh Mesh(string id) => new($"test/{id}", new float[]
    { 0,0,0,1,1,1,1, 1,0,0,1,1,1,1, 0,1,0,1,1,1,1 });
}