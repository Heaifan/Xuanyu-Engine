using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04ZoomGuard
{
    [Fact]
    public void GroundLevelCamera_UsesNearSmallerThanTargetDistanceRule()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(24.5d, 121.5d, 0d), 500d);
        session.Camera.SnapAxis(Vector3.UnitX);

        float near = session.NearPlaneMeters;
        Assert.InRange(near, 0.5f, 2.5f);
        Assert.True(near < session.Camera.DistanceMeters * 0.01d);
    }

    [Fact]
    public void DistantCamera_KeepsOriginalNearRule()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        Assert.InRange(session.NearPlaneMeters, 1499f, 1501f);
    }

    [Fact]
    public void CursorZoom_DoesNotMoveTargetOutsideLoadedHgt()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(24.01d, 121.01d, 0d), 5000d);
        for (int i = 0; i < 24; i++)
            session.ZoomAtCursor(-3f, new Vector2(950f, 600f), 1000, 700);
        GeoCoordinate target = session.Camera.Target;
        Assert.InRange(target.LatitudeDegrees, 24d, 25d);
        Assert.InRange(target.LongitudeDegrees, 121d, 122d);
    }
}