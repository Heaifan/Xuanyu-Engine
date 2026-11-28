using System.Numerics;
using Xunit;
using XuanYu.Editor.Camera;

namespace XuanYu.World.Tests;

public sealed class test_ViewportCamera
{
    [Fact]
    public void Orbit_ShouldChangeDirectionButNotDistance()
    {
        var camera = new ViewportCamera();
        float distance = Vector3.Distance(camera.Position, camera.Target);
        Vector3 before = camera.Position;

        camera.Orbit(80, -30);

        Assert.NotEqual(before, camera.Position);
        Assert.InRange(Vector3.Distance(camera.Position, camera.Target),
            distance - 0.0001f, distance + 0.0001f);
    }

    [Fact]
    public void Pan_ShouldMoveCameraAndTargetTogether()
    {
        var camera = new ViewportCamera();
        Vector3 before = camera.Position - camera.Target;

        camera.Pan(60, -20, 600);

        Assert.NotEqual(Vector3.Zero, camera.Target);
        Assert.InRange(Vector3.Distance(before, camera.Position - camera.Target),
            0f, 0.0001f);
    }

    [Fact]
    public void Zoom_ShouldAdjustDistanceAndStayBounded()
    {
        var camera = new ViewportCamera();
        float distance = Vector3.Distance(camera.Position, camera.Target);

        camera.Zoom(2);
        Assert.True(Vector3.Distance(camera.Position, camera.Target) < distance);
        camera.Zoom(-1000);
        Assert.InRange(Vector3.Distance(camera.Position, camera.Target),
            499.99f, 500.01f);
    }

    [Fact]
    public void Reset_ShouldRestoreDefaultView()
    {
        var camera = new ViewportCamera();
        Vector3 before = camera.Position;
        camera.Orbit(100, 30);
        camera.Pan(20, 40, 700);
        camera.Zoom(4);

        camera.Reset();

        Assert.Equal(Vector3.Zero, camera.Target);
        Assert.InRange(Vector3.Distance(before, camera.Position), 0, 0.0001f);
    }

    [Fact]
    public void Camera_ProjectionShouldChangeAfterOrbit()
    {
        var camera = new ViewportCamera();
        Matrix4x4 before = camera.ViewProjection(800, 600);

        camera.Orbit(30, 15);

        Assert.NotEqual(before, camera.ViewProjection(800, 600));
    }
}