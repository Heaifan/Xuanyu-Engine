using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

internal static class GeoUnifiedSlope
{
    private const double ArcSecond = Math.PI / (180d * 3600d);

    public static bool TryEncode(GeoUnifiedSamples source, int x, int y, short center,
        out short u, out short v)
    {
        var west = source.SampleRaw(Wrap(x - 1), y);
        var east = source.SampleRaw(Wrap(x + 1), y);
        var south = source.SampleRaw(x, y - 1);
        var north = source.SampleRaw(x, y + 1);
        u = v = 0;
        if (west.Status == GeoSampleStatus.NotLoaded || east.Status == GeoSampleStatus.NotLoaded ||
            south.Status == GeoSampleStatus.NotLoaded || north.Status == GeoSampleStatus.NotLoaded)
            return false;
        double phi = (y - 324000) * ArcSecond;
        double sin = Math.Sin(phi);
        double e2 = Wgs84.EccentricitySquared;
        double w = Math.Sqrt(1d - e2 * sin * sin);
        double n = Wgs84.SemiMajorAxisMeters / w;
        double m = n * (1d - e2) / (w * w);
        double gx = y is 0 or 648000 ? 0 :
            Slope(west.Height, center, east.Height, n * Math.Cos(phi) * ArcSecond);
        double gy = Slope(south.Height, center, north.Height, m * ArcSecond);
        // 上半球八面体编码；几何 LOD 不参与坡度采样。
        double sum = 1d + Math.Abs(gx) + Math.Abs(gy);
        u = (short)Math.Round(-gx / sum * short.MaxValue);
        v = (short)Math.Round(-gy / sum * short.MaxValue);
        return true;
    }

    private static double Slope(short? before, short center, short? after, double meters)
    {
        if (before is { } a && after is { } b) return (b - a) / (2d * meters);
        if (after is { } next) return (next - center) / meters;
        if (before is { } previous) return (center - previous) / meters;
        return 0;
    }

    private static int Wrap(int x) => x < 0 ? x + 1296000 : x >= 1296000 ? x - 1296000 : x;
}