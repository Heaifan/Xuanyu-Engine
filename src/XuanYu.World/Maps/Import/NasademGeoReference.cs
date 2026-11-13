
namespace XuanYu.World.Maps;

public sealed class NasademGeoReference
{
    private const double Radius = 6378137.0;
    private const double EccentricitySquared = 0.0066943799901413165;

    public string TileId { get; }
    public int SouthLatitude { get; }
    public int WestLongitude { get; }
    public double WidthMeters { get; }
    public double LengthMeters { get; }

    public NasademGeoReference(NasademTile tile)
    {
        TileId = tile.TileId;
        SouthLatitude = tile.SouthLatitude;
        WestLongitude = tile.WestLongitude;

        double latitude = (SouthLatitude + 0.5) * Math.PI / 180.0;
        double sine = Math.Sin(latitude);
        double factor = Math.Sqrt(
            1.0 - EccentricitySquared * sine * sine);
        double radians = Math.PI / 180.0;

        WidthMeters = Radius / factor *
            Math.Cos(latitude) * radians;
        LengthMeters = Radius * (1.0 - EccentricitySquared) /
            (factor * factor * factor) * radians;
    }

    public (double Latitude, double Longitude) GeoAt(
        double eastMeters, double northMeters) =>
        (SouthLatitude + northMeters / LengthMeters,
         WestLongitude + eastMeters / WidthMeters);

    public (double EastMeters, double NorthMeters) LocalAt(
        double latitude, double longitude) =>
        ((longitude - WestLongitude) * WidthMeters,
         (latitude - SouthLatitude) * LengthMeters);
}
