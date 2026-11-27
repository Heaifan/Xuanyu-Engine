namespace XuanYu.Editor.TerrainEditing;

internal sealed class TerrainRotatedFractalNoise
{
    private readonly byte[] _perm = new byte[512];
    private readonly float[] _gx = new float[256], _gy = new float[256];
    private uint _state;

    public TerrainRotatedFractalNoise(int seed)
    {
        _state = unchecked((uint)seed);
        if (_state == 0) _state = 1;
        var p = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            p[i] = (byte)i;
            double angle = Next() * Math.PI * 2d;
            _gx[i] = (float)Math.Cos(angle);
            _gy[i] = (float)Math.Sin(angle);
        }
        for (int i = 255; i > 0; i--)
        {
            int j = (int)Math.Floor(Next() * (i + 1));
            (p[i], p[j]) = (p[j], p[i]);
        }
        for (int i = 0; i < 512; i++) _perm[i] = p[i & 255];
    }

    private double Next()
    {
        // 对应 JS 的 Math.imul、无符号右移和 32 位溢出。
        unchecked
        {
            _state += 0x6D2B79F5u;
            uint t = (_state ^ (_state >> 15)) * (1u | _state);
            t = (t + ((t ^ (t >> 7)) * (61u | t))) ^ t;
            return (t ^ (t >> 14)) / 4294967296d;
        }
    }

    private double Noise(double x, double y)
    {
        double ix = Math.Floor(x), iy = Math.Floor(y);
        int X = (int)ix & 255, Y = (int)iy & 255;
        double xf = x - ix, yf = y - iy;
        double u = Fade(xf), v = Fade(yf);
        int aa = _perm[_perm[X] + Y], ab = _perm[_perm[X] + Y + 1];
        int ba = _perm[_perm[X + 1] + Y], bb = _perm[_perm[X + 1] + Y + 1];
        double a = Dot(aa, xf, yf), b = Dot(ba, xf - 1d, yf);
        double c = Dot(ab, xf, yf - 1d), d = Dot(bb, xf - 1d, yf - 1d);
        double l = a + u * (b - a), r = c + u * (d - c);
        return l + v * (r - l);
    }

    private double Dot(int i, double x, double y) => _gx[i] * x + _gy[i] * y;
    private static double Fade(double t) => t * t * t * (t * (t * 6d - 15d) + 10d);

    public double Fbm(double x, double y, int octaves) => Fractal(x, y, octaves, 2.03d, 0.5d, false);
    public double Ridged(double x, double y) => Fractal(x, y, 6, 2.15d, 0.53d, true);

    private double Fractal(double x, double y, int octaves, double lacunarity, double gain, bool ridged)
    {
        double sum = 0d, amp = 0.5d, freq = 1d, norm = 0d, weight = 1d;
        const double cosR = 0.7986355d, sinR = 0.6018150d;
        for (int i = 0; i < octaves; i++)
        {
            double n = Noise(x * freq, y * freq);
            if (ridged)
            {
                n = 1d - Math.Abs(n);
                n = n * n * weight;
                weight = Math.Clamp(n * 1.85d, 0d, 1d);
            }
            sum += n * amp; norm += amp;
            freq *= lacunarity; amp *= gain;
            (x, y) = (x * cosR - y * sinR, x * sinR + y * cosR);
        }
        return sum / norm;
    }
}