using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public readonly record struct GeoTileSphere(EcefPosition Center, double RadiusMeters)
{
    public static GeoTileSphere For(GeoTileId id, double heightMarginMeters)
    {
        GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
        double latitude = (bounds.NorthLatitudeDegrees + bounds.SouthLatitudeDegrees) * 0.5d;
        double longitude = (bounds.WestLongitudeDegrees + bounds.EastLongitudeDegrees) * 0.5d;
        EcefPosition center = Wgs84.ToEcef(new GeoCoordinate(latitude, longitude, 0d));
        double halfSpanRadians = (
            bounds.NorthLatitudeDegrees - bounds.SouthLatitudeDegrees +
            bounds.EastLongitudeDegrees - bounds.WestLongitudeDegrees) * Math.PI / 360d;
        // 6400 km 上界覆盖 WGS84 经线/纬线弧长；高度余量由调用方明确提供。
        double radius = Math.Min(2d * Wgs84.SemiMajorAxisMeters,
            6_400_000d * halfSpanRadians) + heightMarginMeters;
        return new GeoTileSphere(center, radius);
    }

    public double DistanceTo(EcefPosition position)
    {
        double dx = Center.X - position.X;
        double dy = Center.Y - position.Y;
        double dz = Center.Z - position.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}