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
}