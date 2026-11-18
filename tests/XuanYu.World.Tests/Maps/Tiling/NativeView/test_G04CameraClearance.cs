using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04CameraClearance
{
    [Fact]
    public void OutsideHgt_CameraCannotZoomBelowEllipsoid()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FocusForCursorZoom(new GeoCoordinate(23d, 120d, 0d));
        session.Camera.Zoom(200d);

        double altitude = Wgs84.ToGeo(session.Camera.PositionEcef).HeightMeters;
        Assert.True(altitude >= 2.99d,
            $"未加载区域不应允许镜头钻入椭球：当前高度 {altitude:F3} m");
    }
}