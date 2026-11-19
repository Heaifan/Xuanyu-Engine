namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoContourElevationTask
{
    private void TraceCell(int x, double a, double b, double c, double d)
    {
        double min = Math.Min(Math.Min(a, b), Math.Min(c, d));
        double max = Math.Max(Math.Max(a, b), Math.Max(c, d));
        int first = (int)Math.Ceiling(min / _style.IntervalMeters);
        int end = (int)Math.Ceiling(max / _style.IntervalMeters);
        Span<GeoContourCoordinate> cuts = stackalloc GeoContourCoordinate[4];
        for (int k = first; k < end; k++)
        {
            double level = (double)k * _style.IntervalMeters;
            int mask = 0;
            if (Cross(a, b, level, _ys[_row], _xs[x], _ys[_row], _xs[x + 1], out cuts[0])) mask |= 1;
            if (Cross(b, c, level, _ys[_row], _xs[x + 1], _ys[_row + 1], _xs[x + 1], out cuts[1])) mask |= 2;
            if (Cross(c, d, level, _ys[_row + 1], _xs[x + 1], _ys[_row + 1], _xs[x], out cuts[2])) mask |= 4;
            if (Cross(d, a, level, _ys[_row + 1], _xs[x], _ys[_row], _xs[x], out cuts[3])) mask |= 8;
            if (mask == 15)
            {
                bool isolateOther = ((a > level) == ((a + b + c + d) * 0.25d > level));
                if (isolateOther) { Add(k, cuts[0], cuts[1]); Add(k, cuts[2], cuts[3]); }
                else { Add(k, cuts[0], cuts[3]); Add(k, cuts[1], cuts[2]); }
            }
            else if (System.Numerics.BitOperations.PopCount((uint)mask) == 2)
            {
                int firstEdge = System.Numerics.BitOperations.TrailingZeroCount((uint)mask);
                int secondEdge = System.Numerics.BitOperations.TrailingZeroCount(
                    (uint)(mask & ~(1 << firstEdge)));
                Add(k, cuts[firstEdge], cuts[secondEdge]);
            }
        }
    }

    private void Add(int level, GeoContourCoordinate a, GeoContourCoordinate b)
    {
        double dy = a.Latitude - b.Latitude, dx = a.Longitude - b.Longitude;
        if (dy * dy + dx * dx > 1e-22d)
            _segments.Add(new GeoContourSegment(level, a, b));
    }

    private static bool Cross(double a, double b, double level, double ya,
        double xa, double yb, double xb, out GeoContourCoordinate point)
    {
        point = default;
        if ((a <= level) == (b <= level)) return false;
        double t = (level - a) / (b - a);
        point = new(ya + (yb - ya) * t, xa + (xb - xa) * t);
        return true;
    }
}