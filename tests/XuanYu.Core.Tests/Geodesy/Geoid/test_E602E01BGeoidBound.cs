using XuanYu.Core.Geodesy;
using Xunit;

namespace XuanYu.Core.Tests.Geodesy;

public sealed class test_E602E01BGeoidBound
{
    [Theory]
    [InlineData(23.4, 23.6, 120.4, 120.6)]
    [InlineData(-0.1, 0.1, 179.9, 180.1)]
    [InlineData(-90, -89.8, -180, -179.8)]
    public void UpperBoundContainsInteriorAndGridCrossings(double south, double north,
        double west, double east)
    {
        var grid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!);
        double bound = grid.UpperBoundMeters(south, north, west, east);
        for (int y = 0; y <= 16; y++)
            for (int x = 0; x <= 16; x++)
            {
                var p = new GeoCoordinate(south + (north - south) * y / 16d,
                    west + (east - west) * x / 16d, 0d);
                Assert.True(grid.UndulationMeters(p) <= bound + 1e-9d);
            }
        var center = new GeoCoordinate((south + north) / 2d, (west + east) / 2d, 0d);
        Assert.Equal(grid.UndulationMeters(center), grid.UpperBoundMeters(
            center.LatitudeDegrees, center.LatitudeDegrees,
            center.LongitudeDegrees, center.LongitudeDegrees), 9);
    }
}