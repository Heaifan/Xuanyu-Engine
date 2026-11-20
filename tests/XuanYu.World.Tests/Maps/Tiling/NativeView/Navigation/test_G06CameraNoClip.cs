using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06CameraNoClip
{
    [Theory]
    [InlineData(-10000d, 10000d)]
    [InlineData(10000d, -10000d)]
    public void VisibleRidge_StopsSegmentAtThreeMeterClearance(double start, double end)
    {
        float[] vertices =
        [
            -20, -10, 0, 0, 1, 0, 1, 0, 10, 100, 0, 1, 0, 1, 0, -10, 100, 0, 1, 0, 1,
            0, -10, 100, 0, 1, 0, 1, 0, 10, 100, 0, 1, 0, 1, 20, -10, 0, 0, 1, 0, 1
        ];
        var surface = new GeoContourSurface(new XuanYu.Render.RenderMesh("ridge", vertices));
        var a = new EnuPosition(start, -5d, 53d);
        var b = new EnuPosition(end, -5d, 53d);
        double t = surface.Sweep(a, b, 3d);
        Assert.Equal(Math.CopySign(10d, start), start + (end - start) * t, 6);
    }

    [Fact]
    public void EmbeddedCamera_CanMoveOutwardToRecoverClearance()
    {
        float[] vertices =
        [
            -10, -10, 10, 1, 1, 1, 1, 10, -10, 10, 1, 1, 1, 1, 10, 10, 10, 1, 1, 1, 1,
            -10, -10, 10, 1, 1, 1, 1, 10, 10, 10, 1, 1, 1, 1, -10, 10, 10, 1, 1, 1, 1
        ];
        var surface = new GeoContourSurface(
            new XuanYu.Render.RenderMesh("embedded-camera", vertices));
        double outward = surface.Sweep(
            new EnuPosition(0, 0, 1), new EnuPosition(0, 0, 20), 3);
        double inward = surface.Sweep(
            new EnuPosition(0, 0, 1), new EnuPosition(0, 0, -20), 3);
        Assert.Equal(1d, outward);
        Assert.Equal(0d, inward);
    }

    [Theory]
    [InlineData(1000d, 20000d, 5640d, 5690d)]
    [InlineData(20000d, 1000d, 5690d, 5740d)]
    public void LongZoom_StopsAtFirstFaceOfSingleColumnHgtRidge(
        double start, double end, double minimum, double maximum)
    {
        using var file = new G02BHgtFixture("N00E000");
        for (int row = 1799; row <= 1801; row++)
            file.SetHeight(1984, row, 700);
        var tile = file.Load();
        var session = new GeoTerrainRenderSession(tile);
        session.Camera.FrameTop(new GeoCoordinate(0.5d, 0.5d, 0d), start);
        session.Camera.SnapAxis(Vector3.UnitX);
        session.Camera.Zoom(Math.Log(end / start) / Math.Log(0.85d));
        Assert.InRange(session.Camera.DistanceMeters, minimum, maximum);
        var eye = Wgs84.ToGeo(session.Camera.PositionEcef);
        var ground = new XuanYu.World.Maps.NasademElevationSource(tile)
            .Sample(eye.LatitudeDegrees, eye.LongitudeDegrees);
        Assert.True(eye.HeightMeters >= ground + 2.999d);
    }

    [Fact]
    public void CursorRefocus_CannotTranslateEyeAcrossSingleColumnRidge()
    {
        using var file = new G02BHgtFixture("N00E000");
        for (int row = 1799; row <= 1801; row++)
            file.SetHeight(1984, row, 700);
        var session = new GeoTerrainRenderSession(file.Load());
        var target = new GeoCoordinate(0.5d, 0.5d, 0d);
        session.Camera.FrameTop(target, 1000d);
        session.Camera.SnapAxis(Vector3.UnitX);
        session.Camera.FocusForCursorZoom(new GeoCoordinate(0.5d, 0.6d, 0d));
        Assert.Equal(target, session.Camera.Target);
    }

    [Fact]
    public void TerrainPan_UsesCollisionAwareFocusTranslation()
    {
        using var file = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(file.Load());
        session.Camera.FrameTop(new GeoCoordinate(24.5d, 121.5d, 0d), 1000d);
        GeoCoordinate before = session.Camera.Target;
        session.Navigate(20f, 0f, 0f, true, 532);
        Assert.NotEqual(before, session.Camera.Target);
    }

    [Fact]
    public void MountainFocus_IsRaisedAboveRealHgt()
    {
        using var file = new G02BHgtFixture("N24E121");
        file.SetHeight(1800, 1800, 2400);
        var session = new GeoTerrainRenderSession(file.Load());
        var peak = new GeoCoordinate(24.5d, 121.5d, 0d);
        session.Camera.FrameTop(peak, 300d);
        Assert.True(session.Camera.Target.HeightMeters >= 2399.9d);
        session.Camera.SnapAxis(Vector3.UnitX);
        Assert.True(session.Camera.PitchRadians >= 0.01d);
        Assert.True(session.Camera.Target.HeightMeters >= 2399.9d);
    }

    [Fact]
    public void CursorFocus_RejectsUndergroundPitchWithoutChangingTarget()
    {
        var camera = new GlobalCamera(new GeoCoordinate(24.5d, 121.5d, 0d), 500d);
        camera.SnapAxis(Vector3.UnitX);
        GeoCoordinate before = camera.Target;
        camera.FocusForCursorZoom(new GeoCoordinate(24.5d, 111.5d, 0d));
        Assert.Equal(before, camera.Target);
        Assert.True(camera.PitchRadians >= 0.01d);
    }

    [Fact]
    public void NearZoom_AgainstMountain_KeepsCameraAndFocusAboveTerrain()
    {
        var camera = new GlobalCamera(new GeoCoordinate(24.5d, 121.5d, 0d), 1000d);
        camera.SetTerrainCollision(_ => 1800d);
        camera.FrameTop(new GeoCoordinate(24.5d, 121.5d, 0d), 200d);
        camera.Zoom(1000d);
        double altitude = Wgs84.ToGeo(camera.PositionEcef).HeightMeters;
        Assert.True(camera.Target.HeightMeters >= 1800d);
        Assert.True(altitude >= 1802.99d,
            $"相机仍低于安全高程：{altitude:F3} m");
    }
}
