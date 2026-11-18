using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G03FNavigation
{
    [Fact]
    public void ZAxis_ResetsNorthAndFramesImportedHgt()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Navigate(380, 180, 3, false, 700);
        session.SnapAxis(Vector3.UnitZ, 1000, 700);
        Assert.InRange(session.Camera.YawRadians, -1e-9, 1e-9);
        Assert.True(session.Camera.PitchRadians > 1.5d);
        Assert.InRange(session.Camera.Target.LatitudeDegrees, 24.4999, 24.5001);
        Assert.InRange(session.Camera.Target.LongitudeDegrees, 121.4999, 121.5001);
        Assert.InRange(session.Camera.DistanceMeters, 100_000d, 160_000d);
    }

    [Fact]
    public void CursorZoom_UsesRealHgtEvenBeforeFirstDraw()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        GeoCoordinate before = session.Camera.Target;
        session.ZoomAtCursor(2, new Vector2(600, 370), 1000, 700);
        GeoCoordinate after = session.Camera.Target;
        Assert.True(session.Camera.DistanceMeters < 150_000d);
        Assert.True(Math.Abs(after.LongitudeDegrees - before.LongitudeDegrees) > 0.00001d ||
            Math.Abs(after.LatitudeDegrees - before.LatitudeDegrees) > 0.00001d);
        Assert.InRange(after.LatitudeDegrees, 24d, 25d);
        Assert.InRange(after.LongitudeDegrees, 121d, 122d);
    }

    [Fact]
    public void CursorZoom_PreservesEarthFixedViewingDirection()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        EcefPosition eyeBefore = session.Camera.PositionEcef;
        EcefPosition targetBefore = session.Camera.TargetEcef;
        GeoCoordinate geoBefore = session.Camera.Target;
        double distanceBefore = session.Camera.DistanceMeters;

        session.ZoomAtCursor(2f, new Vector2(600f, 370f), 1000, 700);

        EcefPosition eyeAfter = session.Camera.PositionEcef;
        EcefPosition targetAfter = session.Camera.TargetEcef;
        GeoCoordinate geoAfter = session.Camera.Target;
        double distanceAfter = session.Camera.DistanceMeters;
        Assert.True(distanceAfter < distanceBefore);
        Assert.True(Math.Abs(geoAfter.LongitudeDegrees - geoBefore.LongitudeDegrees) > 1e-5d ||
            Math.Abs(geoAfter.LatitudeDegrees - geoBefore.LatitudeDegrees) > 1e-5d);
        Assert.InRange(Math.Abs((eyeBefore.X - targetBefore.X) / distanceBefore -
            (eyeAfter.X - targetAfter.X) / distanceAfter), 0d, 1e-8d);
        Assert.InRange(Math.Abs((eyeBefore.Y - targetBefore.Y) / distanceBefore -
            (eyeAfter.Y - targetAfter.Y) / distanceAfter), 0d, 1e-8d);
        Assert.InRange(Math.Abs((eyeBefore.Z - targetBefore.Z) / distanceBefore -
            (eyeAfter.Z - targetAfter.Z) / distanceAfter), 0d, 1e-8d);
    }
}