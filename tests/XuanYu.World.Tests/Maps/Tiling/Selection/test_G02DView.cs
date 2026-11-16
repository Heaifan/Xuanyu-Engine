using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02DView
{
    private static readonly EcefPosition Eye =
        new(3d * Wgs84.SemiMajorAxisMeters, 0d, 0d);
    private static readonly EcefPosition EarthCenter = new(0d, 0d, 0d);

    [Fact]
    public void LookingAtEarth_SeesBothRootRegions()
    {
        var view = new GeoTileView(Eye, EarthCenter, Math.PI / 3d, 1600, 900);
        var selector = new GeoTileSelector(100_000d, 2d, maxLevel: 0);
        Assert.Equal(2, selector.Select(view).Count);
    }

    [Fact]
    public void LookingAwayFromEarth_CullsAllRoots()
    {
        var away = new EcefPosition(4d * Wgs84.SemiMajorAxisMeters, 0d, 0d);
        var view = new GeoTileView(Eye, away, Math.PI / 3d, 1600, 900);
        var selector = new GeoTileSelector(100_000d, 2d);
        Assert.Empty(selector.Select(view));
    }

    [Fact]
    public void View_RejectsInvalidViewportAndDirection()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTileView(Eye, EarthCenter, Math.PI / 3d, 0, 900));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTileView(Eye, EarthCenter, Math.PI / 3d, 1600, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTileView(Eye, EarthCenter, Math.PI, 1600, 900));
        Assert.Throws<ArgumentException>(() =>
            new GeoTileView(Eye, Eye, Math.PI / 3d, 1600, 900));
    }
}