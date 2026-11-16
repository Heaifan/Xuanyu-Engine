
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;

namespace XuanYu.World.Tests.Geodesy;

public sealed class test_GlobalCamera
{
    [Fact]
    public void OrbitRadius_UsesRealMeters()
    {
        var camera = new GlobalCamera(new GeoCoordinate(23.5, 121.5, 0),
            10_000_000);
        EcefPosition eye = camera.PositionEcef;
        EcefPosition target = camera.TargetEcef;
        double dx = eye.X - target.X;
        double dy = eye.Y - target.Y;
        double dz = eye.Z - target.Z;

        Assert.InRange(Math.Abs(Math.Sqrt(dx * dx + dy * dy + dz * dz) -
            10_000_000d), 0d, 0.001d);
    }

    [Fact]
    public void CameraRelative_UsesLocalMeters()
    {
        var camera = new GlobalCamera(new GeoCoordinate(23.5, 121.5, 0),
            10_000_000);
        var frame = new EnuFrame(camera.Target);
        var atOrigin = camera.ToCameraRelative(frame.OriginEcef);
        var tenMetersEast = camera.ToCameraRelative(
            frame.ToEcef(new EnuPosition(10, 0, 0)));

        Assert.InRange(Math.Abs(tenMetersEast.X - atOrigin.X - 10f),
            0f, 0.001f);
        Assert.Equal(System.Numerics.Vector3.Zero,
            camera.ToCameraRelative(camera.PositionEcef));
    }

    [Fact]
    public void Zoom_DoesNotModifyPhysicalGeographicTarget()
    {
        var target = new GeoCoordinate(23.5, 121.5, 0);
        var camera = new GlobalCamera(target, 10_000_000);

        camera.Zoom(5);
        Assert.True(camera.DistanceMeters < 10_000_000d);
        Assert.Equal(target, camera.Target);
        camera.Zoom(-1000);
        Assert.Equal(GlobalCamera.MaxDistanceMeters, camera.DistanceMeters);
    }

    [Fact]
    public void Orbit_ChangesCameraPositionAndKeepsTarget()
    {
        var target = new GeoCoordinate(23.5, 121.5, 0);
        var camera = new GlobalCamera(target);
        EcefPosition before = camera.PositionEcef;

        camera.Orbit(0.5, 0.1);

        Assert.NotEqual(before, camera.PositionEcef);
        Assert.Equal(target, camera.Target);
    }
}
