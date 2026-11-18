using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal sealed class GeoTerrainFootprintGrid
{
    private readonly HashSet<(int South, int West)> _loaded;
    private readonly GeoTileBounds _coverage;
    private readonly int _minimumLevel;

    public GeoTerrainFootprintGrid(NasademHgtInfo[] tiles, GeoTileBounds coverage)
    {
        _loaded = tiles.Select(t => (t.SouthLatitude, t.WestLongitude)).ToHashSet();
        _coverage = coverage;
        double span = Math.Max(coverage.NorthLatitudeDegrees - coverage.SouthLatitudeDegrees,
            coverage.EastLongitudeDegrees - coverage.WestLongitudeDegrees);
        _minimumLevel = Math.Max(0, (int)Math.Ceiling(Math.Log2(180d / span)));
    }

    public GeoTileBounds Clip(GeoTileId id)
    {
        GeoTileBounds b = GeographicTileScheme.Bounds(id);
        return new(Math.Max(b.SouthLatitudeDegrees, _coverage.SouthLatitudeDegrees),
            Math.Max(b.WestLongitudeDegrees, _coverage.WestLongitudeDegrees),
            Math.Min(b.NorthLatitudeDegrees, _coverage.NorthLatitudeDegrees),
            Math.Min(b.EastLongitudeDegrees, _coverage.EastLongitudeDegrees));
    }

    public bool Covers(double latitude, double longitude) =>
        _loaded.Contains(((int)Math.Floor(latitude), (int)Math.Floor(longitude)));

    public bool IsFull(GeoTileBounds b)
    {
        for (int lat = (int)Math.Floor(b.SouthLatitudeDegrees); lat < b.NorthLatitudeDegrees; lat++)
            for (int lon = (int)Math.Floor(b.WestLongitudeDegrees); lon < b.EastLongitudeDegrees; lon++)
                if (!_loaded.Contains((lat, lon))) return false;
        return true;
    }

    public double[] Axis(GeoTileId id, double min, double max, bool horizontal,
        bool native, bool cut, IEnumerable<double> extra)
    {
        double origin = horizontal ? -180d : -90d;
        double step = 180d / (1L << (Math.Max(id.Level, _minimumLevel) + 4));
        var points = new List<double> { min, max };
        // Native 的内部点仅采用原始 1 角秒采样及必要的共边节点。
        if (native) AddLattice(points, min, max, Math.Floor(min), 1d / 3600d);
        else AddLattice(points, min, max, origin, step);
        if (cut) AddLattice(points, min, max, 0d, 1d);
        points.AddRange(extra.Where(v => v > min && v < max));
        points.Sort();
        var unique = new List<double>(points.Count);
        foreach (double p in points)
            if (unique.Count == 0 || p - unique[^1] > 1e-11d) unique.Add(p);
        unique[0] = min;
        unique[^1] = max;
        return unique.ToArray();
    }

    private static void AddLattice(List<double> points, double min, double max,
        double origin, double step)
    {
        int first = (int)Math.Ceiling((min - origin) / step);
        int last = (int)Math.Floor((max - origin) / step);
        for (int i = first; i <= last; i++)
        {
            double value = origin + i * step;
            if (value > min && value < max) points.Add(value);
        }
    }

    public IEnumerable<double> EdgeCuts(double min, double max, double fixedValue,
        bool horizontal)
    {
        int low = (int)Math.Floor(Math.BitDecrement(fixedValue));
        int high = (int)Math.Floor(Math.BitIncrement(fixedValue));
        for (int i = (int)Math.Floor(min) + 1; i < max; i++)
            if (Changes(low, i, horizontal) || Changes(high, i, horizontal)) yield return i;
    }

    private bool Changes(int fixedCell, int split, bool horizontal) => horizontal
        ? _loaded.Contains((fixedCell, split - 1)) != _loaded.Contains((fixedCell, split))
        : _loaded.Contains((split - 1, fixedCell)) != _loaded.Contains((split, fixedCell));
}