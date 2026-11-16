
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Geodesy;

public sealed class test_NasademGeoGrid
{
    [Fact]
    public void N23E121_UsesExactDegreeBounds()
    {
        var grid = new NasademGeoGrid(23, 121);
        var southWest = grid.AtSample(0, 0, 0);
        var center = grid.AtSample(1800, 1800, 120);
        var northEast = grid.AtSample(3600, 3600, 0);

        Assert.Equal(23d, southWest.LatitudeDegrees);
        Assert.Equal(121d, southWest.LongitudeDegrees);
        Assert.Equal(23.5d, center.LatitudeDegrees);
        Assert.Equal(121.5d, center.LongitudeDegrees);
        Assert.Equal(120d, center.HeightMeters);
        Assert.Equal(24d, northEast.LatitudeDegrees);
        Assert.Equal(122d, northEast.LongitudeDegrees);
    }

    [Fact]
    public void AdjacentTiles_ShareExactLongitudeBoundary()
    {
        var west = new NasademGeoGrid(23, 121);
        var east = new NasademGeoGrid(23, 122);

        Assert.Equal(west.AtSample(3600, 1234, 0),
            east.AtSample(0, 1234, 0));
    }

    [Fact]
    public void Antimeridian_IsNormalizedByGeographicCoordinate()
    {
        var grid = new NasademGeoGrid(10, 179);
        Assert.Equal(-180d, grid.AtSample(3600, 1800, 0).LongitudeDegrees);
    }

    [Fact]
    public void InvalidSample_IsRejected()
    {
        var grid = new NasademGeoGrid(23, 121);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            grid.AtSample(3601, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            grid.AtSample(0, -1, 0));
    }
}
