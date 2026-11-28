using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.Ecs.Tests.Ecs;

public sealed class test_E602ASpatialPosition
{
    [Fact]
    public void LocalPositionPreservesCentimetersAtMillionMeters()
    {
        var origin = SpatialPosition.FromLocal(1_000_000, -2_000_000, 3_000_000);
        var point = SpatialPosition.FromLocal(1_000_000.01, -1_999_999.98, 2_999_999.97);
        var delta = point.RelativeTo(origin);
        Assert.Equal(SpatialSpace.Local, point.Space);
        Assert.InRange(Math.Abs(delta.X - 0.01), 0, 1e-8);
        Assert.InRange(Math.Abs(delta.Y - 0.02), 0, 1e-8);
        Assert.InRange(Math.Abs(delta.Z + 0.03), 0, 1e-8);
        Assert.Throws<InvalidOperationException>(() => point.ToEcef());
        Assert.Throws<InvalidOperationException>(() =>
            point.RelativeTo(SpatialPosition.FromGeo(new GeoCoordinate(0, 0, 0))));
    }

    [Fact]
    public void GeoRoundTripPreservesLatitudeLongitudeAndHeight()
    {
        var geo = new GeoCoordinate(24.5, 121.5, 100);
        var point = SpatialPosition.FromGeo(geo);
        var result = point.ToGeo();
        Assert.Equal(SpatialSpace.Ecef, point.Space);
        Assert.InRange(Math.Abs(result.LatitudeDegrees - geo.LatitudeDegrees), 0, 1e-9);
        Assert.InRange(Math.Abs(result.LongitudeDegrees - geo.LongitudeDegrees), 0, 1e-9);
        Assert.InRange(Math.Abs(result.HeightMeters - geo.HeightMeters), 0, 1e-5);
    }

    [Fact]
    public void EcefPositionPreservesRawDoubleValues()
    {
        var ecef = new EcefPosition(6_378_137.01, -0.02, 0.03);
        var point = SpatialPosition.FromEcef(ecef);
        Assert.Equal(ecef, point.ToEcef());
        var delta = point.RelativeTo(SpatialPosition.FromEcef(new EcefPosition(6_378_137, 0, 0)));
        Assert.InRange(Math.Abs(delta.X - 0.01), 0, 1e-8);
        Assert.Equal(-0.02, delta.Y);
        Assert.Equal(0.03, delta.Z);
    }

    [Fact]
    public void EnuFrameRoundTripPreservesCentimeterOffsets()
    {
        var frame = new EnuFrame(new GeoCoordinate(24.5, 121.5, 100));
        var local = new EnuPosition(0.01, -0.02, 0.03);
        var result = SpatialPosition.FromEnu(frame, local).ToEnu(frame);
        Assert.InRange(Math.Abs(result.East - local.East), 0, 1e-8);
        Assert.InRange(Math.Abs(result.North - local.North), 0, 1e-8);
        Assert.InRange(Math.Abs(result.Up - local.Up), 0, 1e-8);
    }

    [Fact]
    public void DateLineCrossingUsesContinuousEcefSpace()
    {
        var frame = new EnuFrame(new GeoCoordinate(0, 179.999, 0));
        var point = SpatialPosition.FromGeo(new GeoCoordinate(0, -179.999, 0));
        var enu = point.ToEnu(frame);
        Assert.InRange(enu.East, 222.6, 222.7);
        Assert.InRange(Math.Abs(enu.North), 0, 1e-8);
        Assert.InRange(enu.Up, -0.005, 0);
        Assert.InRange(Math.Abs(point.ToGeo().LongitudeDegrees + 179.999), 0, 1e-9);
    }
}