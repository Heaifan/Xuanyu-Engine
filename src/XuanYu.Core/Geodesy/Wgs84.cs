
namespace XuanYu.Core.Geodesy;

public static class Wgs84
{
    public const double SemiMajorAxisMeters = 6378137.0;
    public const double InverseFlattening = 298.257223563;
    public const double Flattening = 1d / InverseFlattening;
    public const double SemiMinorAxisMeters = SemiMajorAxisMeters * (1d - Flattening);
    public const double EccentricitySquared = Flattening * (2d - Flattening);
    public const double SecondEccentricitySquared =
        (SemiMajorAxisMeters * SemiMajorAxisMeters -
         SemiMinorAxisMeters * SemiMinorAxisMeters) /
        (SemiMinorAxisMeters * SemiMinorAxisMeters);

    public static EcefPosition ToEcef(GeoCoordinate geo)
    {
        double latitude = geo.LatitudeDegrees * Math.PI / 180d;
        double longitude = geo.LongitudeDegrees * Math.PI / 180d;
        double sinLatitude = Math.Sin(latitude);
        double cosLatitude = Math.Cos(latitude);
        double radius = SemiMajorAxisMeters / Math.Sqrt(
            1d - EccentricitySquared * sinLatitude * sinLatitude);

        return new EcefPosition(
            (radius + geo.HeightMeters) * cosLatitude * Math.Cos(longitude),
            (radius + geo.HeightMeters) * cosLatitude * Math.Sin(longitude),
            (radius * (1d - EccentricitySquared) + geo.HeightMeters) * sinLatitude);
    }

    public static GeoCoordinate ToGeo(EcefPosition ecef)
    {
        if (!double.IsFinite(ecef.X) || !double.IsFinite(ecef.Y) ||
            !double.IsFinite(ecef.Z))
            throw new ArgumentOutOfRangeException(nameof(ecef));

        double horizontal = Math.Sqrt(ecef.X * ecef.X + ecef.Y * ecef.Y);
        if (horizontal < 1e-8)
        {
            if (ecef.Z == 0d)
                throw new ArgumentException("地心无法确定大地纬度。", nameof(ecef));
            return new GeoCoordinate(Math.CopySign(90d, ecef.Z), 0d,
                Math.Abs(ecef.Z) - SemiMinorAxisMeters);
        }

        double theta = Math.Atan2(ecef.Z * SemiMajorAxisMeters,
            horizontal * SemiMinorAxisMeters);
        double sinTheta = Math.Sin(theta);
        double cosTheta = Math.Cos(theta);
        double latitude = Math.Atan2(
            ecef.Z + SecondEccentricitySquared * SemiMinorAxisMeters *
                sinTheta * sinTheta * sinTheta,
            horizontal - EccentricitySquared * SemiMajorAxisMeters *
                cosTheta * cosTheta * cosTheta);

        for (int i = 0; i < 3; i++)
        {
            double sine = Math.Sin(latitude);
            double radius = SemiMajorAxisMeters / Math.Sqrt(
                1d - EccentricitySquared * sine * sine);
            latitude = Math.Atan2(
                ecef.Z + EccentricitySquared * radius * sine, horizontal);
        }

        double sinLatitude = Math.Sin(latitude);
        double cosLatitude = Math.Cos(latitude);
        double height = horizontal * cosLatitude + ecef.Z * sinLatitude -
            SemiMajorAxisMeters * Math.Sqrt(
                1d - EccentricitySquared * sinLatitude * sinLatitude);
        return new GeoCoordinate(latitude * 180d / Math.PI,
            Math.Atan2(ecef.Y, ecef.X) * 180d / Math.PI, height);
    }
}
