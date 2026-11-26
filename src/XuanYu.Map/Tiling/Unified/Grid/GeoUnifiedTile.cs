namespace XuanYu.World.Maps;

public readonly record struct GeoUnifiedTile
{
    public int Level { get; }
    public int X { get; }
    public int Y { get; }
    public int Span => 32 << Level;
    public int West => X * Span;
    public int South => Y * Span;

    public GeoUnifiedTile(int level, int x, int y)
    {
        if (level is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(level));
        int span = 32 << level;
        if (x < 0 || (long)x * span >= 1_296_000) throw new ArgumentOutOfRangeException(nameof(x));
        if (y < 0 || (long)y * span >= 648_000) throw new ArgumentOutOfRangeException(nameof(y));
        Level = level; X = x; Y = y;
    }

    public GeoUnifiedTile Parent() => new(Level + 1, X / 2, Y / 2);

    public GeoUnifiedTile Child(int dx, int dy)
    {
        if (Level == 0) throw new InvalidOperationException("最细地形块没有子块。");
        if ((uint)dx > 1) throw new ArgumentOutOfRangeException(nameof(dx));
        if ((uint)dy > 1) throw new ArgumentOutOfRangeException(nameof(dy));
        return new GeoUnifiedTile(Level - 1, X * 2 + dx, Y * 2 + dy);
    }
}
