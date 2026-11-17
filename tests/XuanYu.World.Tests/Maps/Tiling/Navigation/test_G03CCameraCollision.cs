using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G03CCameraCollision
{
    [Fact]
    public void RepeatedZoom_CannotEnterRealNasademSurface()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        for (int i = 0; i < 8; i++)
            session.Navigate(0, 0, 30, false, 700);

        GeoCoordinate eye = Wgs84.ToGeo(session.Camera.PositionEcef);
        Assert.InRange(eye.HeightMeters, 2.99d, 150_000d);
        double pitch = session.Camera.PitchRadians;
        session.Navigate(0, -1000, 0, false, 700);
        Assert.Equal(pitch, session.Camera.PitchRadians);

        double before = session.Camera.DistanceMeters;
        session.Navigate(0, 0, -3, false, 700);
        Assert.True(session.Camera.DistanceMeters > before);
    }

    [Fact]
    public void FocusAndOrbit_RejectUnsafeCameraPositions()
    {
        var original = new GeoCoordinate(24.5d, 121.5d, 0d);
        var camera = new GlobalCamera(original, 20d);
        camera.SetTerrainCollision(location =>
            location.LatitudeDegrees > 24.505d ? 1000d : 0d);
        camera.Focus(new GeoCoordinate(24.51d, 121.5d, 0d));
        Assert.Equal(original, camera.Target);

        camera.Zoom(200d);
        GeoCoordinate eye = Wgs84.ToGeo(camera.PositionEcef);
        Assert.InRange(eye.HeightMeters, 2.99d, 20d);
        double pitch = camera.PitchRadians;
        camera.Orbit(0d, -5d);
        Assert.Equal(pitch, camera.PitchRadians);
    }
}