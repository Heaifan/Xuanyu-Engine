
namespace XuanYu.Core.Geodesy;

public sealed class EnuFrame
{
    private readonly double _sinLatitude;
    private readonly double _cosLatitude;
    private readonly double _sinLongitude;
    private readonly double _cosLongitude;

    public GeoCoordinate Origin { get; }
    public EcefPosition OriginEcef { get; }

    public EnuFrame(GeoCoordinate origin)
    {
        Origin = origin;
        OriginEcef = Wgs84.ToEcef(origin);
        double latitude = origin.LatitudeDegrees * Math.PI / 180d;
        double longitude = origin.LongitudeDegrees * Math.PI / 180d;
        _sinLatitude = Math.Sin(latitude);
        _cosLatitude = Math.Cos(latitude);
        _sinLongitude = Math.Sin(longitude);
        _cosLongitude = Math.Cos(longitude);
    }

    public EnuPosition ToEnu(EcefPosition point)
    {
        double dx = point.X - OriginEcef.X;
        double dy = point.Y - OriginEcef.Y;
        double dz = point.Z - OriginEcef.Z;
        return new EnuPosition(
            -_sinLongitude * dx + _cosLongitude * dy,
            -_sinLatitude * _cosLongitude * dx -
                _sinLatitude * _sinLongitude * dy + _cosLatitude * dz,
            _cosLatitude * _cosLongitude * dx +
                _cosLatitude * _sinLongitude * dy + _sinLatitude * dz);
    }

    public EcefPosition ToEcef(EnuPosition point)
    {
        double dx = -_sinLongitude * point.East -
            _sinLatitude * _cosLongitude * point.North +
            _cosLatitude * _cosLongitude * point.Up;
        double dy = _cosLongitude * point.East -
            _sinLatitude * _sinLongitude * point.North +
            _cosLatitude * _sinLongitude * point.Up;
        double dz = _cosLatitude * point.North + _sinLatitude * point.Up;
        return new EcefPosition(
            OriginEcef.X + dx, OriginEcef.Y + dy, OriginEcef.Z + dz);
    }
}
