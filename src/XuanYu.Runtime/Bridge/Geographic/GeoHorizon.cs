using XuanYu.Core.Geodesy;

namespace XuanYu.Runtime;

internal static class GeoHorizon
{
    public static bool Visible(EcefPosition eye, EcefPosition target)
    {
        const double a = Wgs84.SemiMajorAxisMeters, b = Wgs84.SemiMinorAxisMeters;
        double x = eye.X/a, y = eye.Y/a, z = eye.Z/b;
        double dx = target.X/a-x, dy = target.Y/a-y, dz = target.Z/b-z;
        double length = dx*dx + dy*dy + dz*dz;
        if (length == 0) return true;
        double t = -(x*dx+y*dy+z*dz)/length;
        // 只剔除视线中途穿过椭球的符号；不因终点处的负高程隐藏近侧标记。
        if (t <= 0 || t >= 1) return true;
        x += t*dx; y += t*dy; z += t*dz;
        return x*x+y*y+z*z >= 1d-1e-10;
    }
}