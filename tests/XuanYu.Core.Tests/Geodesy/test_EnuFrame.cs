
using Xunit;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Tests.Geodesy;

public sealed class test_EnuFrame
{
    [Fact]
    public void Origin_ConvertsToZeroMeters()
    {
        var frame = new EnuFrame(new GeoCoordinate(24.5, 121.5, 100));
        EnuPosition local = frame.ToEnu(frame.OriginEcef);

        Assert.Equal(0d, local.East);
        Assert.Equal(0d, local.North);
        Assert.Equal(0d, local.Up);
    }

    [Fact]
    public void EquatorAxes_MatchEarthCenteredDirections()
    {
        var frame = new EnuFrame(new GeoCoordinate(0, 0, 0));
        EcefPosition east = frame.ToEcef(new EnuPosition(1, 0, 0));
        EcefPosition north = frame.ToEcef(new EnuPosition(0, 1, 0));
        EcefPosition up = frame.ToEcef(new EnuPosition(0, 0, 1));

        Assert.Equal(1d, east.Y, 10);
        Assert.Equal(1d, north.Z, 10);
        Assert.Equal(Wgs84.SemiMajorAxisMeters + 1d, up.X, 6);
    }

    [Theory]
    [InlineData(0d, 0d)]
    [InlineData(24.5d, 121.5d)]
    [InlineData(-60d, -179.9d)]
    [InlineData(89.9d, 179.9d)]
    public void EnuEcefRoundTrip_HoldsWithinOneMillimeter(
        double latitude, double longitude)
    {
        var frame = new EnuFrame(new GeoCoordinate(latitude, longitude, 500));
        var source = new EnuPosition(100000, -40000, 3000);
        EnuPosition result = frame.ToEnu(frame.ToEcef(source));

        Assert.InRange(Math.Abs(result.East - source.East), 0d, 0.001d);
        Assert.InRange(Math.Abs(result.North - source.North), 0d, 0.001d);
        Assert.InRange(Math.Abs(result.Up - source.Up), 0d, 0.001d);
    }

    [Fact]
    public void EnuDistance_IsPhysicalMeters()
    {
        var frame = new EnuFrame(new GeoCoordinate(35, 110, 250));
        var local = new EnuPosition(100000, -40000, 3000);
        EcefPosition ecef = frame.ToEcef(local);
        double dx = ecef.X - frame.OriginEcef.X;
        double dy = ecef.Y - frame.OriginEcef.Y;
        double dz = ecef.Z - frame.OriginEcef.Z;
        double ecefMeters = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        double localMeters = Math.Sqrt(local.East * local.East +
            local.North * local.North + local.Up * local.Up);

        Assert.InRange(Math.Abs(ecefMeters - localMeters), 0d, 1e-6);
    }
}
