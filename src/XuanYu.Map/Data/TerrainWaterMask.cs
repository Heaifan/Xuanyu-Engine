namespace XuanYu.World.Maps;

public sealed class TerrainWaterMask
{
    private readonly byte[] _values;
    public int SamplesX { get; }
    public int SamplesY { get; }
    public ReadOnlySpan<byte> Samples => _values;

    public TerrainWaterMask(int samplesX, int samplesY, byte[] values)
    {
        if (samplesX < 2 || samplesY < 2 ||
            values.Length != checked(samplesX * samplesY))
            throw new ArgumentException("水体掩膜尺寸不匹配。", nameof(values));
        if (values.Any(value => value is not (0 or 255)))
            throw new ArgumentException("SWB 只能包含 0 或 255。", nameof(values));
        SamplesX = samplesX;
        SamplesY = samplesY;
        _values = values.ToArray();
    }

    public bool IsWater(int x, int y) => _values[y * SamplesX + x] == 255;

    public bool IsCoastLand(int x, int y)
    {
        if (IsWater(x, y)) return false;
        for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
            {
                int nx = x + dx, ny = y + dy;
                if ((uint)nx < (uint)SamplesX &&
                    (uint)ny < (uint)SamplesY && IsWater(nx, ny))
                    return true;
            }
        return false;
    }
}