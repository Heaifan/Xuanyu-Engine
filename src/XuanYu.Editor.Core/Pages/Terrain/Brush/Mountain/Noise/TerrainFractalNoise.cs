namespace XuanYu.Editor.TerrainEditing;

public sealed class TerrainFractalNoise
{
    private readonly TerrainGradientNoise _gradient;
    private readonly double _wavelengthMeters;
    private readonly int _octaves;

    public TerrainFractalNoise(int seed, double wavelengthMeters, int octaves)
    {
        if (!double.IsFinite(wavelengthMeters) || wavelengthMeters <= 0d)
            throw new ArgumentOutOfRangeException(nameof(wavelengthMeters));
        if (octaves is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(octaves));
        _gradient = new(seed);
        _wavelengthMeters = wavelengthMeters;
        _octaves = octaves;
    }

    // 米制输入；各倍频须落在梯度噪声的有效索引范围内。
    public double Sample(double xMeters, double yMeters) =>
        SampleFractal(xMeters, yMeters, ridged: false);

    public double SampleRidged(double xMeters, double yMeters) =>
        SampleFractal(xMeters, yMeters, ridged: true);

    private double SampleFractal(double xMeters, double yMeters, bool ridged)
    {
        double x = xMeters / _wavelengthMeters, y = yMeters / _wavelengthMeters;
        double sum = 0d, amplitude = 1d, totalAmplitude = 0d, weight = 1d;
        for (int i = 0; i < _octaves; i++)
        {
            double value = _gradient.Sample(x, y);
            if (ridged)
            {
                double ridge = 1d - Math.Abs(value);
                value = ridge * ridge * weight;
                weight = Math.Min(1d, 2d * value);
            }
            sum += value * amplitude;
            totalAmplitude += amplitude;
            amplitude *= 0.5d;
            // 每倍频旋转采样轴，减少网格方向在多层噪声中的叠加。
            (x, y) = (2d * (0.8d * x - 0.6d * y), 2d * (0.6d * x + 0.8d * y));
        }
        return sum / totalAmplitude;
    }
}