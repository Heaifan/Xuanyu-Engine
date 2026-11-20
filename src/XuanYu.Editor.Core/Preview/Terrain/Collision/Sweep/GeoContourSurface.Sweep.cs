using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoContourSurface
{
    public double Sweep(EnuPosition a, EnuPosition b, double clearance)
    {
        double dx = b.East - a.East, dy = b.North - a.North;
        double minY = Math.Min(a.North, b.North), maxY = Math.Max(a.North, b.North);
        if (maxY < _minY || minY > _minY + _spanY) return 1d;
        double first = 1d;
        var visited = new HashSet<int>();
        for (int row = BinY((float)minY); row <= BinY((float)maxY); row++)
        {
            double lo = 0d, hi = first;
            double south = _minY + (double)_spanY * row / _side;
            double north = _minY + (double)_spanY * (row + 1) / _side;
            if (!Clip(a.North - south, dy, ref lo, ref hi) ||
                !Clip(north - a.North, -dy, ref lo, ref hi)) continue;
            double x0 = a.East + dx * lo, x1 = a.East + dx * hi;
            if (Math.Max(x0, x1) < _minX || Math.Min(x0, x1) > _minX + _spanX) continue;
            int left = BinX((float)Math.Min(x0, x1)), right = BinX((float)Math.Max(x0, x1));
            for (int col = left; col <= right; col++)
                if (_buckets[row * _side + col] is { } candidates)
                    foreach (int i in candidates)
                        if (visited.Add(i)) first = Math.Min(first,
                            HitTriangle(_mesh.Vertices.Span.Slice(i, 21), a, b, clearance));
        }
        return first;
    }

    private static double HitTriangle(ReadOnlySpan<float> v, EnuPosition a,
        EnuPosition b, double clearance)
    {
        double ax = v[0], ay = v[1], bx = v[7], by = v[8], cx = v[14], cy = v[15];
        double det = (by - cy) * (ax - cx) + (cx - bx) * (ay - cy);
        double dx = b.East - a.East, dy = b.North - a.North;
        double u = ((by - cy) * (a.East - cx) + (cx - bx) * (a.North - cy)) / det;
        double w = ((cy - ay) * (a.East - cx) + (ax - cx) * (a.North - cy)) / det;
        double du = ((by - cy) * dx + (cx - bx) * dy) / det;
        double dw = ((cy - ay) * dx + (ax - cx) * dy) / det;
        double lo = 0d, hi = 1d;
        if (!Clip(u, du, ref lo, ref hi) || !Clip(w, dw, ref lo, ref hi) ||
            !Clip(1d - u - w, -du - dw, ref lo, ref hi)) return 1d;
        double ac = (double)v[2] - v[16], bc = (double)v[9] - v[16];
        double gap = a.Up - clearance - (v[16] + u * ac + w * bc);
        double slope = b.Up - a.Up - (du * ac + dw * bc);
        if (gap + slope * lo < 0d)
            return lo <= 0d && slope > 0d ? 1d : lo;
        return gap + slope * hi < 0d ? Math.Clamp(-gap / slope, lo, hi) : 1d;
    }

    // Restrict [lo, hi] to the halfspace value + t * slope >= 0.
    private static bool Clip(double value, double slope, ref double lo, ref double hi)
    {
        if (slope == 0d) return value >= 0d;
        double at = -value / slope;
        if (slope > 0d) lo = Math.Max(lo, at);
        else hi = Math.Min(hi, at);
        return lo <= hi;
    }
}
