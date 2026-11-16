
namespace XuanYu.Core.Geodesy;

public readonly record struct GeoCoordinate
{
    public double LatitudeDegrees { get; }
    public double LongitudeDegrees { get; }
    public double HeightMeters { get; }

    public GeoCoordinate(double latitudeDegrees, double longitudeDegrees,
        double heightMeters)
    {
        if (!double.IsFinite(latitudeDegrees) ||
            latitudeDegrees < -90d || latitudeDegrees > 90d)
            throw new ArgumentOutOfRangeException(nameof(latitudeDegrees));
        if (!double.IsFinite(longitudeDegrees))
            throw new ArgumentOutOfRangeException(nameof(longitudeDegrees));
        if (!double.IsFinite(heightMeters))
            throw new ArgumentOutOfRangeException(nameof(heightMeters));

        LatitudeDegrees = latitudeDegrees;
        double longitude = longitudeDegrees % 360d;
        LongitudeDegrees = longitude >= 180d ? longitude - 360d :
            longitude < -180d ? longitude + 360d : longitude;
        // 高度为 WGS84 椭球高，不能直接代入 NASADEM 的正高。
        HeightMeters = heightMeters;
    }
}
