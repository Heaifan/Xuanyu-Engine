
using System.Numerics;
using Xunit;
using XuanYu.Editor.Camera;

namespace XuanYu.World.Tests.Camera;

public sealed class TestViewportScreenRay
{
    [Fact]
    public void Perspective_CenterRayPointsToTarget()
    {
        var camera = new ViewportCamera();
        bool found = ViewportScreenRay.TryCreate(
            camera.ViewProjection(800, 600),
            new Vector2(400, 300), 800, 600,
            out _, out var direction);

        Assert.True(found);
        Vector3 forward = Vector3.Normalize(
            camera.Target - camera.Position);
        Assert.True(Vector3.Dot(direction, forward) > 0.9999f);
    }

    [Fact]
    public void UpperPixelPointsTowardsCameraUp()
    {
        var camera = new ViewportCamera();
        Matrix4x4 matrix = camera.ViewProjection(800, 600);

        Assert.True(ViewportScreenRay.TryCreate(matrix,
            new Vector2(400, 300), 800, 600,
            out _, out var center));
        Assert.True(ViewportScreenRay.TryCreate(matrix,
            new Vector2(400, 150), 800, 600,
            out _, out var upper));

        Assert.True(Vector3.Dot(
            upper - center, camera.ViewUp) > 0f);
    }

    [Fact]
    public void Orthographic_RaysAreParallel()
    {
        var camera = new ViewportCamera();
        camera.ToggleProjection();
        Matrix4x4 matrix = camera.ViewProjection(800, 600);

        Assert.True(ViewportScreenRay.TryCreate(matrix,
            new Vector2(200, 300), 800, 600,
            out var left, out var leftDirection));
        Assert.True(ViewportScreenRay.TryCreate(matrix,
            new Vector2(600, 300), 800, 600,
            out var right, out var rightDirection));

        Assert.True(Vector3.Distance(left, right) > 0.1f);
        Assert.True(Vector3.Dot(
            leftDirection, rightDirection) > 0.9999f);
    }

    [Fact]
    public void InvalidViewport_ReturnsFalse()
    {
        Assert.False(ViewportScreenRay.TryCreate(
            Matrix4x4.Identity, Vector2.Zero, 0, 600,
            out _, out _));

        Assert.False(ViewportScreenRay.TryCreate(
            Matrix4x4.Identity, new Vector2(-1, 5),
            800, 600, out _, out _));
    }
}
