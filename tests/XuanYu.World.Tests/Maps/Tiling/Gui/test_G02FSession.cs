using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02FSession
{
    [Fact]
    public void RealHgt_ProducesDrawsAndRespondsToCamera()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        var draws = session.Draws(1000, 700);
        for (int i = 0; i < 32 && draws.Count == 0; i++)
            draws = session.Draws(1000, 700);
        Assert.NotEmpty(draws);
        Assert.True(session.SelectedCount > 0);
        Assert.All(draws, draw => Assert.True(draw.Visible));
        var before = draws[0].ModelToClip;

        session.Navigate(0, 0, 5, false, 700);
        var after = session.Draws(1000, 700);
        Assert.NotEmpty(after);
        Assert.NotEqual(before, after[0].ModelToClip);
    }

    [Fact]
    public void WorldSelector_CanLimitRequestsToImportedTileArea()
    {
        var center = XuanYu.Core.Geodesy.Wgs84.ToEcef(
            new XuanYu.Core.Geodesy.GeoCoordinate(23.5d, 120.5d, 0d));
        var eye = XuanYu.Core.Geodesy.Wgs84.ToEcef(
            new XuanYu.Core.Geodesy.GeoCoordinate(23.5d, 120.5d, 150_000d));
        var view = new GeoTileView(eye, center, Math.PI / 3d, 800, 600);
        var selector = new GeoTileSelector(200_000d, 4d, 13, 64);
        var region = new GeoTileBounds(23d, 120d, 24d, 121d);
        var selected = selector.Select(view, region);
        Assert.NotEmpty(selected);
        Assert.All(selected, tile =>
        {
            GeoTileBounds bounds = GeographicTileScheme.Bounds(tile);
            Assert.True(bounds.WestLongitudeDegrees < region.EastLongitudeDegrees);
            Assert.True(bounds.EastLongitudeDegrees > region.WestLongitudeDegrees);
        });
    }
}