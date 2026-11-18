namespace XuanYu.World.Maps;

public enum MapVerticalDatum : byte
{
    Wgs84Ellipsoid = 0,
    Egm96Orthometric = 1
}

public sealed record MapGeoReference
{
    public double SouthLatitudeDegrees { get; }
    public double WestLongitudeDegrees { get; }
    public double NorthLatitudeDegrees { get; }
    public double EastLongitudeDegrees { get; }
    public MapVerticalDatum VerticalDatum { get; }

    public MapGeoReference(double southLatitudeDegrees,
        double westLongitudeDegrees, double northLatitudeDegrees,
        double eastLongitudeDegrees, MapVerticalDatum verticalDatum)
    {
        if (!double.IsFinite(southLatitudeDegrees) ||
            !double.IsFinite(northLatitudeDegrees) ||
            southLatitudeDegrees < -90d || northLatitudeDegrees > 90d ||
            southLatitudeDegrees >= northLatitudeDegrees)
            throw new ArgumentOutOfRangeException(nameof(southLatitudeDegrees));
        if (!double.IsFinite(westLongitudeDegrees) ||
            !double.IsFinite(eastLongitudeDegrees) ||
            westLongitudeDegrees < -180d || eastLongitudeDegrees > 180d ||
            westLongitudeDegrees >= eastLongitudeDegrees)
            throw new ArgumentOutOfRangeException(nameof(westLongitudeDegrees));
        if (!Enum.IsDefined(verticalDatum))
            throw new ArgumentOutOfRangeException(nameof(verticalDatum));

        SouthLatitudeDegrees = southLatitudeDegrees;
        WestLongitudeDegrees = westLongitudeDegrees;
        NorthLatitudeDegrees = northLatitudeDegrees;
        EastLongitudeDegrees = eastLongitudeDegrees;
        VerticalDatum = verticalDatum;
    }
}