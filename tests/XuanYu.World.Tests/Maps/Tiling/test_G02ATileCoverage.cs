using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02ATileCoverage
{
    [Fact]
    public void NasademN23E120_CoversNineTilesAtLevelEight()
    {
        var source = new MapGeoReference(23, 120, 24, 121,
            MapVerticalDatum.Egm96Orthometric);
        var extent = new GeoTileBounds(source.SouthLatitudeDegrees,
            source.WestLongitudeDegrees, source.NorthLatitudeDegrees,
            source.EastLongitudeDegrees);
        GeoTileId[] tiles = GeographicTileScheme.Cover(extent, 8).ToArray();

        Assert.Equal(9, tiles.Length);
        foreach (GeoTileId tile in tiles)
        {
            GeoTileBounds bounds = GeographicTileScheme.Bounds(tile);
            Assert.True(bounds.WestLongitudeDegrees < 121 &&
                bounds.EastLongitudeDegrees > 120);
            Assert.True(bounds.SouthLatitudeDegrees < 24 &&
                bounds.NorthLatitudeDegrees > 23);
        }
    }

    [Fact]
    public void ExactBoundary_DoesNotSelectTouchingTiles()
    {
        GeoTileBounds oneTile = GeographicTileScheme.Bounds(
            new GeoTileId(4, 10, 5));
        GeoTileId[] tiles = GeographicTileScheme.Cover(oneTile, 4).ToArray();
        Assert.Equal(new GeoTileId(4, 10, 5), Assert.Single(tiles));
    }

    [Fact]
    public void WholeEarth_ListsBothRootsWithoutInventingExtraTiles()
    {
        var globe = new GeoTileBounds(-90, -180, 90, 180);
        GeoTileId[] expected = [new GeoTileId(0, 0, 0), new GeoTileId(0, 1, 0)];
        Assert.Equal(expected,
            GeographicTileScheme.Cover(globe, 0).ToArray());
    }

    [Fact]
    public void InvalidOrCrossingExtent_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeographicTileScheme.Cover(
                new GeoTileBounds(0, 179, 10, -179), 8).ToArray());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeographicTileScheme.Cover(
                new GeoTileBounds(0, 0, double.NaN, 10), 8).ToArray());
    }
}