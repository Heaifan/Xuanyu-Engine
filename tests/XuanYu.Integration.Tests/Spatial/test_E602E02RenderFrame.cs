using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World;
using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E602E02RenderFrame
{
    [Fact]
    public void DistantAnchorRetainsCentimeterMovement()
    {
        var anchor = new EnuFrame(new GeoCoordinate(0, 0, 0));
        var eye = new EnuPosition(1_000_000, 2_000_000, 30_000);
        var target = anchor.ToEcef(new(eye.East, eye.North + 20, eye.Up - 10));
        var frame = new GeoRenderFrame(anchor, anchor.ToEcef(eye), target, 1, 0.5f, 500_000);
        var p = frame.ToRelative(anchor.ToEcef(new(eye.East + 0.01, eye.North + 1, eye.Up - 0.02)));
        Assert.InRange(p.X, 0.009999f, 0.010001f);
        Assert.InRange(p.Y, 0.999999f, 1.000001f);
        Assert.InRange(p.Z, -0.020001f, -0.019999f);
        Vector4 center = Vector4.Transform(new Vector4(frame.ToRelative(target), 1), frame.RelativeProjection);
        Assert.InRange(MathF.Abs(center.X / center.W), 0, 1e-6f);
        Assert.InRange(MathF.Abs(center.Y / center.W), 0, 1e-6f);
    }

    [Fact]
    public void GeographicSnapshotUsesEntityLocalEnuAndNeverMutatesWorld()
    {
        var anchor = new EnuFrame(new GeoCoordinate(0, 0, 0));
        var entityFrame = new EnuFrame(new GeoCoordinate(30, 40, 100));
        var position = SpatialPosition.FromEcef(entityFrame.OriginEcef);
        var frame = new GeoRenderFrame(anchor, entityFrame.ToEcef(new(0, -20, 10)),
            entityFrame.OriginEcef, 1, 0.5f, 500_000);
        var world = new WorldScene();
        var global = world.Create("global", position);
        var local = world.Create("local");
        var bridge = new WorldRenderBridge(world);
        var mesh = new RenderMesh("test/cube", new float[21]);
        bridge.Bind(global.Id, mesh);
        bridge.Bind(local.Id, mesh);
        world.SetTransform(global.Id, Transform.FromSpatial(position,
            Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 2), new Vector3(2, 1, 1)));
        var draw = Assert.Single(bridge.SnapshotGeographic(frame, global.Id));
        Assert.True(draw.Selected);
        var expected = Vector4.Transform(new Vector4(frame.ToRelative(entityFrame.ToEcef(new(0, 2, 0))), 1),
            frame.RelativeProjection);
        var actual = Vector4.Transform(new Vector4(1, 0, 0, 1), draw.ModelToClip);
        Assert.InRange(Vector4.Distance(expected, actual), 0, 1e-4f);
        Assert.Equal(position, world.GetSpatialPosition(global.Id));
        world.SetVisible(global.Id, false);
        Assert.False(Assert.Single(bridge.SnapshotGeographic(frame)).Visible);
        Assert.Single(bridge.Snapshot(Matrix4x4.Identity));
    }
}