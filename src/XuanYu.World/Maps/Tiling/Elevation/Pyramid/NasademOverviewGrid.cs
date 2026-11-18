namespace XuanYu.World.Maps;

public sealed class NasademOverviewGrid
{
    private readonly short[] _samples;
    public NasademHgtInfo Info { get; }
    public NasademOverviewLevel Level { get; }
    public int Side { get; }
    public int Stride => (int)Level;
    public long SampleBytes => (long)_samples.Length * sizeof(short);
    internal ReadOnlySpan<short> Samples => _samples;

    internal NasademOverviewGrid(NasademHgtInfo info, NasademOverviewLevel level,
        int side, short[] samples)
    {
        Info = info;
        Level = level;
        Side = side;
        _samples = samples;
    }

    public short? HeightAt(int column, int rowFromSouth)
    {
        if ((uint)column >= (uint)Side || (uint)rowFromSouth >= (uint)Side)
            throw new ArgumentOutOfRangeException(nameof(column));
        short value = _samples[rowFromSouth * Side + column];
        return value == short.MinValue ? null : value;
    }
}