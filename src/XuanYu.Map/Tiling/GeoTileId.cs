namespace XuanYu.World.Maps;

public readonly record struct GeoTileId
{
    public const int MaxLevel = 29;

    public int Level { get; }
    public int X { get; }
    public int Y { get; }

    public GeoTileId(int level, int x, int y)
    {
        if (level is < 0 or > MaxLevel)
            throw new ArgumentOutOfRangeException(nameof(level));
        if ((uint)x >= (uint)(2 << level))
            throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)(1 << level))
            throw new ArgumentOutOfRangeException(nameof(y));
        Level = level;
        X = x;
        Y = y;
    }

    public override string ToString() => $"{Level}/{X}/{Y}";
}