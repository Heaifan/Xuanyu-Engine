using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed partial class test_G06CameraNoClip
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
}