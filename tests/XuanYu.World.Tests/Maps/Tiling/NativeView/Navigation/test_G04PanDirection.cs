using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04PanDirection
{
    [Fact]
    public void DragDown_MovesFocusNorth_ForDefaultView()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        GeoCoordinate before = session.Camera.Target;
        session.Navigate(0f, 30f, 0f, true, 700);
        Assert.True(session.Camera.Target.LatitudeDegrees >
            before.LatitudeDegrees);
        session.Navigate(0f, -30f, 0f, true, 700);
        Assert.InRange(session.Camera.Target.LatitudeDegrees,
            before.LatitudeDegrees - 0.0001d,
            before.LatitudeDegrees + 0.0001d);
    }

    [Fact]
    public void DragRight_MovesFocusWest_ForDefaultView()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        GeoCoordinate before = session.Camera.Target;
        session.Navigate(30f, 0f, 0f, true, 700);
        Assert.True(session.Camera.Target.LongitudeDegrees <
            before.LongitudeDegrees);
    }
}