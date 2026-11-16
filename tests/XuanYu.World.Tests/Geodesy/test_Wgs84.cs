
using Xunit;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Tests.Geodesy;

public sealed class test_Wgs84
{
    [Fact]
    public void ReferenceAxes_UseWgs84Meters()
    {
        EcefPosition equator = Wgs84.ToEcef(new GeoCoordinate(0, 0, 0));
        EcefPosition pole = Wgs84.ToEcef(new GeoCoordinate(90, 0, 0));

        Assert.Equal(6378137d, equator.X, 6);
        Assert.InRange(Math.Abs(equator.Y), 0d, 1e-8);
        Assert.InRange(Math.Abs(equator.Z), 0d, 1e-8);
        Assert.InRange(Math.Abs(pole.X), 0d, 1e-8);
        Assert.InRange(Math.Abs(pole.Z - Wgs84.SemiMinorAxisMeters), 0d, 1e-8);
    }

    [Theory]
    [InlineData(0d, 0d, 0d)]
    [InlineData(24.5d, 121.5d, 750d)]
    [InlineData(-33.86d, 151.21d, -430d)]
    [InlineData(89.999999d, -179.999d, 2000d)]
    [InlineData(30d, 179.999999d, 1000000d)]
    [InlineData(0d, 181d, 1000d)]
    public void GeoEcefRoundTrip_IsAccurate(double latitude, double longitude,
        double height)
    {
        var source = new GeoCoordinate(latitude, longitude, height);
        GeoCoordinate actual = Wgs84.ToGeo(Wgs84.ToEcef(source));

        Assert.InRange(Math.Abs(actual.LatitudeDegrees - source.LatitudeDegrees),
            0d, 1e-9);
        double deltaLongitude = (actual.LongitudeDegrees -
            source.LongitudeDegrees + 540d) % 360d - 180d;
        Assert.InRange(Math.Abs(deltaLongitude), 0d, 1e-9);
        Assert.InRange(Math.Abs(actual.HeightMeters - source.HeightMeters),
            0d, 0.001d);
    }

    [Theory]
    [InlineData(90d, 120d, 200d)]
    [InlineData(-90d, -130d, -500d)]
    public void ExactPole_KeepsLatitudeAndHeight(double latitude,
        double longitude, double height)
    {
        var actual = Wgs84.ToGeo(Wgs84.ToEcef(
            new GeoCoordinate(latitude, longitude, height)));
        Assert.Equal(latitude, actual.LatitudeDegrees);
        Assert.InRange(Math.Abs(actual.HeightMeters - height), 0d, 0.001d);
    }

    [Fact]
    public void Longitude_WrapsAcrossAntimeridian()
    {
        Assert.Equal(-179d, new GeoCoordinate(0, 181, 0).LongitudeDegrees);
        Assert.Equal(179d, new GeoCoordinate(0, -181, 0).LongitudeDegrees);
        Assert.Equal(-180d, new GeoCoordinate(0, 180, 0).LongitudeDegrees);
    }

    [Fact]
    public void InvalidLatitudeAndUndefinedGeocenter_AreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoCoordinate(90.01, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoCoordinate(0, 0, double.NaN));
        Assert.Throws<ArgumentException>(() =>
            Wgs84.ToGeo(new EcefPosition(0, 0, 0)));
    }
}
