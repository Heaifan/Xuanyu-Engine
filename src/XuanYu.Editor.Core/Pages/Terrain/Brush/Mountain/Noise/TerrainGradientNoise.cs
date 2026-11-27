namespace XuanYu.Editor.TerrainEditing;

public sealed class TerrainGradientNoise(int seed)
{
    // 输入为噪声空间坐标；调用方保证有限，且下取整后的格点及其 +1 邻点可由 int 表示。
    public double Sample(double x, double y)
    {
        int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y);
        double fx = x - ix, fy = y - iy;
        double u = Fade(fx), v = Fade(fy);
        double a = Dot(ix, iy, fx, fy);
        double b = Dot(ix + 1, iy, fx - 1d, fy);
        double c = Dot(ix, iy + 1, fx, fy - 1d);
        double d = Dot(ix + 1, iy + 1, fx - 1d, fy - 1d);
        return Lerp(Lerp(a, b, u), Lerp(c, d, u), v) * 0.7071067811865475d;
    }

    private double Dot(int x, int y, double dx, double dy)
    {
        const double diagonal = 0.7071067811865475d;
        return (Hash(x, y) & 7u) switch
        {
            0u => dx,
            1u => -dx,
            2u => dy,
            3u => -dy,
            4u => (dx + dy) * diagonal,
            5u => (dx - dy) * diagonal,
            6u => (-dx + dy) * diagonal,
            _ => (-dx - dy) * diagonal
        };
    }

    private uint Hash(int x, int y)
    {
        unchecked
        {
            uint h = (uint)seed ^ ((uint)x * 0x9E3779B1u) ^ ((uint)y * 0x85EBCA77u);
            h ^= h >> 16;
            h *= 0x7FEB352Du;
            h ^= h >> 15;
            h *= 0x846CA68Bu;
            return h ^ (h >> 16);
        }
    }

    private static double Fade(double t) => t * t * t * (t * (t * 6d - 15d) + 10d);
    private static double Lerp(double a, double b, double t) => a + (b - a) * t;
}