namespace XuanYu.Editor.TerrainEditing;

internal sealed partial class TerrainRainGrid
{
    private readonly float[] _target;
    private readonly double[] _heights;
    private readonly int _width;
    private readonly double _floor;
    private readonly float[]? _weights;
    public int Height { get; }
    public double Sx { get; }
    public double Sy { get; }
    public double Step => Math.Min(Sx, Sy);

    public TerrainRainGrid(float[] heights, int width, float sx, float sy, float[]? weights)
    {
        _target = heights;
        _weights = weights;
        _heights = Array.ConvertAll(heights, h => (double)h);
        _width = width;
        Height = heights.Length / width;
        Sx = sx;
        Sy = sy;
        _floor = _heights.Min();
    }

    public bool Contains(double x, double y) =>
        x >= 1 && y >= 1 && x < _width - 2 && y < Height - 2 &&
        (_weights is null || MaskWeights(x, y).Total > 0d);

    public (double H, double Gx, double Gy) Sample(double x, double y)
    {
        int ix = (int)x, iy = (int)y, i = iy * _width + ix;
        double u = x - ix, v = y - iy;
        double a = _heights[i], b = _heights[i + 1];
        double c = _heights[i + _width], d = _heights[i + _width + 1];
        double top = a + (b - a) * u, bottom = c + (d - c) * u;
        return (top + (bottom - top) * v,
            ((b - a) * (1 - v) + (d - c) * v) / Sx, (bottom - top) / Sy);
    }

    public double Erode(double x, double y, double amount)
    {
        int cx = (int)(x + 0.5), cy = (int)(y + 0.5);
        double removed = 0;
        for (int oy = -1; oy <= 1; oy++)
        for (int ox = -1; ox <= 1; ox++)
        {
            int ix = cx + ox, iy = cy + oy;
            if (ix < 1 || iy < 1 || ix >= _width - 1 || iy >= Height - 1) continue;
            int r2 = ox * ox + oy * oy;
            double weight = r2 == 0 ? 0.36 : r2 == 1 ? 0.11 : 0.05;
            int i = iy * _width + ix;
            double cut = Math.Min(amount * weight * (_weights?[i] ?? 1f), _heights[i] - _floor);
            _heights[i] -= cut;
            removed += cut;
        }
        return removed;
    }

    public void Deposit(double x, double y, double amount)
    {
        if (amount == 0) return;
        if (_weights is not null) { DepositMasked(x, y, amount); return; }
        int ix = (int)x, iy = (int)y, i = iy * _width + ix;
        double u = x - ix, v = y - iy;
        _heights[i] += amount * (1 - u) * (1 - v);
        _heights[i + 1] += amount * u * (1 - v);
        _heights[i + _width] += amount * (1 - u) * v;
        _heights[i + _width + 1] += amount * u * v;
    }

    public TerrainRainErosion.Result Commit(int droplets, int steps)
    {
        float maxCut = 0, maxFill = 0;
        double cut = 0, fill = 0;
        for (int i = 0; i < _target.Length; i++)
        {
            float next = (float)_heights[i];
            double delta = (double)next - _target[i];
            maxCut = Math.Max(maxCut, (float)-delta);
            maxFill = Math.Max(maxFill, (float)delta);
            cut += Math.Max(0, -delta);
            fill += Math.Max(0, delta);
            _target[i] = next;
        }
        return new(droplets, steps, maxCut, maxFill, cut * Sx * Sy, fill * Sx * Sy);
    }
}