using System.Numerics;
using Xunit;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_WorldRenderBridge
{
    [Fact]
    public void Snapshot_ShouldUseWorldIdentity()
    {
        var world = new WorldScene();
        var entity = world.Create("Cube");
        var mesh = Mesh("cube");
        var bridge = new WorldRenderBridge(world);
        bridge.Bind(entity.Id, mesh);

        RenderDraw draw = Assert.Single(
            bridge.Snapshot(Matrix4x4.Identity, entity.Id));

        Assert.Same(mesh, draw.Mesh);
        Assert.Equal(Matrix4x4.Identity, draw.ModelToClip);
        Assert.True(draw.Visible);
        Assert.True(draw.Selected);
    }

    [Fact]
    public void Snapshot_ShouldReadUpdatedTransform()
    {
        var world = new WorldScene();
        var entity = world.Create("Cube");
        var bridge = new WorldRenderBridge(world);
        bridge.Bind(entity.Id, Mesh("cube"));

        RenderDraw before = Assert.Single(
            bridge.Snapshot(Matrix4x4.Identity));

        world.SetTransform(entity.Id, new Transform(
            new Vector3(2.5f, 0f, 0f),
            Quaternion.CreateFromAxisAngle(
                Vector3.UnitZ, MathF.PI / 2f),
            new Vector3(2f, 1f, 1f)));

        RenderDraw after = Assert.Single(
            bridge.Snapshot(Matrix4x4.CreateScale(0.2f)));

        Vector3 point = Vector3.Transform(
            Vector3.UnitX, after.ModelToClip);

        Assert.Equal(Matrix4x4.Identity, before.ModelToClip);
        Assert.InRange(point.X, 0.499f, 0.501f);
        Assert.InRange(point.Y, 0.399f, 0.401f);
    }

    [Fact]
    public void Snapshot_ShouldReadVisibilityAndSelection()
    {
        var world = new WorldScene();
        var a = world.Create("A");
        var b = world.Create("B");
        var bridge = new WorldRenderBridge(world);
        bridge.Bind(a.Id, Mesh("a"));
        bridge.Bind(b.Id, Mesh("b"));

        world.SetVisible(b.Id, false);
        RenderDraw[] draws = bridge.Snapshot(
            Matrix4x4.Identity, a.Id);

        Assert.Equal(2, draws.Length);
        Assert.True(draws[0].Visible);
        Assert.True(draws[0].Selected);
        Assert.False(draws[1].Visible);
        Assert.False(draws[1].Selected);
    }

    [Fact]
    public void Snapshot_ShouldSkipUnboundAndDeleted()
    {
        var world = new WorldScene();
        var a = world.Create("A");
        var b = world.Create("B");
        var bridge = new WorldRenderBridge(world);
        bridge.Bind(a.Id, Mesh("a"));

        Assert.Single(bridge.Snapshot(Matrix4x4.Identity));

        world.Delete(a.Id);

        Assert.Empty(bridge.Snapshot(Matrix4x4.Identity));
        Assert.True(bridge.Unbind(a.Id));

        bridge.Bind(b.Id, Mesh("b"));
        Assert.Single(bridge.Snapshot(Matrix4x4.Identity));
    }

    private static RenderMesh Mesh(string name) =>
        new($"test/{name}", new float[21]);
}