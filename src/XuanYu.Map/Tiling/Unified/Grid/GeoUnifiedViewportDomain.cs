using System.Numerics;

namespace XuanYu.World.Maps;

public sealed class GeoUnifiedViewportDomain
{
    public const int RootLevel = 6;
    private readonly (int South, int West)[] _cells;
    public IReadOnlyList<GeoUnifiedTile> Roots { get; }

    public GeoUnifiedViewportDomain(IEnumerable<(int South, int West)> coordinates)
    {
        _cells = coordinates.Distinct().OrderBy(p => p.South).ThenBy(p => p.West)
            .Select(p => ((p.South + 90) * 3600, (p.West + 180) * 3600)).ToArray();
        if (_cells.Length == 0) throw new ArgumentException("G07 数据目录为空。", nameof(coordinates));
        const int span = 32 << RootLevel;
        var roots = new HashSet<GeoUnifiedTile>();
        foreach (var (south, west) in _cells)
            for (int y = south / span; y <= (south + 3599) / span; y++)
                for (int x = west / span; x <= (west + 3599) / span; x++)
                    roots.Add(new GeoUnifiedTile(RootLevel, x, y));
        Roots = Array.AsReadOnly(roots.OrderBy(t => t.Y).ThenBy(t => t.X).ToArray());
    }

    public bool Contains(double latitude, double longitude)
    {
        double x = (longitude + 180d) * 3600d, y = (latitude + 90d) * 3600d;
        return _cells.Any(c => x >= c.West && x <= c.West + 3600 &&
            y >= c.South && y <= c.South + 3600);
    }

    public bool Intersects(GeoUnifiedTile tile) => _cells.Any(c =>
        tile.West < c.West + 3600 && tile.West + tile.Span > c.West &&
        tile.South < c.South + 3600 && tile.South + tile.Span > c.South);

    public Vector4[] ClipRects(GeoUnifiedTile tile)
    {
        float step = 1 << tile.Level;
        var result = new List<Vector4>(4);
        foreach (var (south, west) in _cells)
        {
            int x0 = Math.Max(tile.West, west), x1 = Math.Min(tile.West + tile.Span, west + 3600);
            int y0 = Math.Max(tile.South, south), y1 = Math.Min(tile.South + tile.Span, south + 3600);
            if (x0 >= x1 || y0 >= y1) continue;
            result.Add(new((x0 - tile.West) / step, (y0 - tile.South) / step,
                (x1 - tile.West) / step, (y1 - tile.South) / step));
        }
        return result.ToArray();
    }

    // 仅用于覆盖边缘的几何插值；未导入区域仍由 GPU 裁剪。
    internal (int X, int Y) Nearest(int x, int y)
    {
        long best = long.MaxValue;
        (int X, int Y) point = default;
        foreach (var (south, west) in _cells)
        {
            int cx = Math.Clamp(x, west, west + 3600);
            int cy = Math.Clamp(y, south, south + 3600);
            long dx = (long)x - cx, dy = (long)y - cy;
            long distance = dx * dx + dy * dy;
            if (distance >= best) continue;
            best = distance; point = (cx, cy);
        }
        return point;
    }
}