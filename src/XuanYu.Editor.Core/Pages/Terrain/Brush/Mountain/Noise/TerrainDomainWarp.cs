namespace XuanYu.Editor.TerrainEditing;

public sealed class TerrainDomainWarp
{
    private readonly TerrainFractalNoise _xNoise, _yNoise;
    private readonly double _amplitudeMeters;

    public TerrainDomainWarp(int seed, double wavelengthMeters, int octaves,
        double amplitudeMeters)
    {
        if (!double.IsFinite(amplitudeMeters) || amplitudeMeters < 0d)
            throw new ArgumentOutOfRangeException(nameof(amplitudeMeters));
        _xNoise = new(seed, wavelengthMeters, octaves);
        _yNoise = new(seed ^ 0x5A17C9E3, wavelengthMeters, octaves);
        _amplitudeMeters = amplitudeMeters;
    }

    // 调用方保证原始及扭曲后的坐标满足后续噪声采样的有效范围。
    public (double X, double Y) Warp(double xMeters, double yMeters)
    {
        if (_amplitudeMeters == 0d) return (xMeters, yMeters);
        return (xMeters + _amplitudeMeters * _xNoise.Sample(xMeters, yMeters),
            yMeters + _amplitudeMeters * _yNoise.Sample(xMeters, yMeters));
    }
}