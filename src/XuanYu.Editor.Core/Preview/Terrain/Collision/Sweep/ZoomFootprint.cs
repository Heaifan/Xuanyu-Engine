using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal readonly record struct ZoomFootprint(
    double South, double North, double Longitude, double LongitudeRadius)
{
    public static ZoomFootprint Around(GeoCoordinate center, double radius)
    {
        // Bound the geodetic coordinate change over the entire half-segment.
        // 6,000 km is below both WGS84 curvature radii throughout HGT terrain.
        const double degrees = 180d / Math.PI;
        double angle = radius / 6_000_000d * degrees;
        double latitude = Math.Abs(center.LatitudeDegrees) + angle;
        double lon = latitude >= 90d ? 180d :
            Math.Min(180d, angle / Math.Cos(latitude / degrees));
        return new(center.LatitudeDegrees - angle, center.LatitudeDegrees + angle,
            center.LongitudeDegrees, lon);
    }

    public double LongitudeNear(double meridian) =>
        meridian + Math.IEEERemainder(Longitude - meridian, 360d);

    public bool Overlaps(GeoTileBounds tile)
    {
        double lon = LongitudeNear((tile.WestLongitudeDegrees + tile.EastLongitudeDegrees) * 0.5d);
        return North >= tile.SouthLatitudeDegrees && South <= tile.NorthLatitudeDegrees &&
            lon + LongitudeRadius >= tile.WestLongitudeDegrees &&
            lon - LongitudeRadius <= tile.EastLongitudeDegrees;
    }
}
