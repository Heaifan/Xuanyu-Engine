using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    public Vector3? SampleSurfaceNormal(double latitudeDegrees, double longitudeDegrees)
    {
        if (Sample(latitudeDegrees, longitudeDegrees) is not { } center) return null;
        double step = (_overview is null ? 1 : (int)_overviewLevel) / 3600d;
        if (Math.Abs(latitudeDegrees) + step >= 90d) return null;
        (double eastMeters, double northMeters) = SurfaceSampleSpacing(latitudeDegrees, step);
        double? west = Sample(latitudeDegrees, WrapNormalLongitude(longitudeDegrees - step));
        double? east = Sample(latitudeDegrees, WrapNormalLongitude(longitudeDegrees + step));
        double? south = Sample(latitudeDegrees - step, longitudeDegrees);
        double? north = Sample(latitudeDegrees + step, longitudeDegrees);
        double? gx = SurfaceSlope(west, center, east, eastMeters);
        double? gy = SurfaceSlope(south, center, north, northMeters);
        if (gx is not { } x || gy is not { } y) return null;
        return Vector3.Normalize(new Vector3((float)-x, (float)-y, 1f));
    }

    private static (double East, double North) SurfaceSampleSpacing(double latitude, double step)
    {
        double phi = latitude * Math.PI / 180d;
        double radians = step * Math.PI / 180d;
        double sin = Math.Sin(phi);
        double w = Math.Sqrt(1d - Wgs84.EccentricitySquared * sin * sin);
        double n = Wgs84.SemiMajorAxisMeters / w;
        double m = n * (1d - Wgs84.EccentricitySquared) / (w * w);
        return (n * Math.Cos(phi) * radians, m * radians);
    }

    private static double? SurfaceSlope(double? before, double center, double? after, double meters)
    {
        // NoData 不能当成零高程；只有单侧有效时才降级为单侧差分。
        if (before is { } a && after is { } b) return (b - a) / (2d * meters);
        if (after is { } next) return (next - center) / meters;
        if (before is { } previous) return (center - previous) / meters;
        return null;
    }

    private static double WrapNormalLongitude(double longitude) =>
        longitude > 180d ? longitude - 360d : longitude < -180d ? longitude + 360d : longitude;
}