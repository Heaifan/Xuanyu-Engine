namespace XuanYu.Editor.TerrainEditing;

internal sealed partial class TerrainRainGrid
{
    private (double A, double B, double C, double D, double Total) MaskWeights(double x, double y)
    {
        int ix = (int)x, iy = (int)y, i = iy * _width + ix;
        double u = x - ix, v = y - iy;
        var weights = _weights!;
        double a = (1 - u) * (1 - v) * weights[i];
        double b = u * (1 - v) * weights[i + 1];
        double c = (1 - u) * v * weights[i + _width];
        double d = u * v * weights[i + _width + 1];
        return (a, b, c, d, a + b + c + d);
    }

    private void DepositMasked(double x, double y, double amount)
    {
        int i = (int)y * _width + (int)x;
        var w = MaskWeights(x, y);
        // Contains 保证沉积点至少有一个有效接收样本；全部载沙必须落在掩膜内。
        _heights[i] += amount * (w.A / w.Total);
        _heights[i + 1] += amount * (w.B / w.Total);
        _heights[i + _width] += amount * (w.C / w.Total);
        _heights[i + _width + 1] += amount * (w.D / w.Total);
    }
}