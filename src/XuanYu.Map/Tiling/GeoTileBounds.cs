namespace XuanYu.World.Maps;

public readonly record struct GeoTileBounds(
    double SouthLatitudeDegrees,
    double WestLongitudeDegrees,
    double NorthLatitudeDegrees,
    double EastLongitudeDegrees);