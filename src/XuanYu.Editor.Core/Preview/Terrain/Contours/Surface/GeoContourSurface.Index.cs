namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoContourSurface
{
    private void BuildIndex()
    {
        ReadOnlySpan<float> data = _mesh.Vertices.Span;
        for (int i = 0; i < data.Length; i += 21)
        {
            float ax = data[i], ay = data[i + 1];
            float bx = data[i + 7], by = data[i + 8];
            float cx = data[i + 14], cy = data[i + 15];
            double det = (by - cy) * (double)(ax - cx) +
                (cx - bx) * (double)(ay - cy);
            if (Math.Abs(det) < 1e-10d) continue;
            int x0 = BinX(MathF.Min(ax, MathF.Min(bx, cx)));
            int x1 = BinX(MathF.Max(ax, MathF.Max(bx, cx)));
            int y0 = BinY(MathF.Min(ay, MathF.Min(by, cy)));
            int y1 = BinY(MathF.Max(ay, MathF.Max(by, cy)));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    int cell = y * _side + x;
                    (_buckets[cell] ??= []).Add(i);
                }
        }
    }

    private float? RecoverNearEdge(float x, float y)
    {
        // 只补亚米级浮点/接缝误差，不跨越真正缺少网格的区域。
        float radius = Math.Clamp(SpacingMeters * 0.02f, 0.5f, 2f);
        if (x < _minX - radius || y < _minY - radius ||
            x > _minX + _spanX + radius ||
            y > _minY + _spanY + radius) return null;
        double best = radius * (double)radius;
        float? height = null;
        ReadOnlySpan<float> data = _mesh.Vertices.Span;
        int bx = BinX(x), by = BinY(y);
        for (int row = Math.Max(0, by - 1); row <= Math.Min(_side - 1, by + 1); row++)
            for (int col = Math.Max(0, bx - 1); col <= Math.Min(_side - 1, bx + 1); col++)
            {
                List<int>? ids = _buckets[row * _side + col];
                if (ids is null) continue;
                foreach (int i in ids)
                {
                    float x0 = MathF.Min(data[i], MathF.Min(data[i + 7], data[i + 14]));
                    float x1 = MathF.Max(data[i], MathF.Max(data[i + 7], data[i + 14]));
                    float y0 = MathF.Min(data[i + 1], MathF.Min(data[i + 8], data[i + 15]));
                    float y1 = MathF.Max(data[i + 1], MathF.Max(data[i + 8], data[i + 15]));
                    if (x < x0 - radius || x > x1 + radius ||
                        y < y0 - radius || y > y1 + radius) continue;
                    NearestEdge(data, i, i + 7, x, y, ref best, ref height);
                    NearestEdge(data, i + 7, i + 14, x, y, ref best, ref height);
                    NearestEdge(data, i + 14, i, x, y, ref best, ref height);
                }
            }
        return height;
    }

    private static void NearestEdge(ReadOnlySpan<float> data, int a, int b,
        float x, float y, ref double best, ref float? height)
    {
        double dx = (double)data[b] - data[a];
        double dy = (double)data[b + 1] - data[a + 1];
        double lengthSquared = dx * dx + dy * dy;
        if (lengthSquared == 0d) return;
        double t = Math.Clamp((((double)x - data[a]) * dx +
            ((double)y - data[a + 1]) * dy) / lengthSquared, 0d, 1d);
        double ex = x - (data[a] + t * dx);
        double ey = y - (data[a + 1] + t * dy);
        double squared = ex * ex + ey * ey;
        if (squared >= best) return;
        best = squared;
        height = (float)(data[a + 2] + t * (data[b + 2] - data[a + 2]));
    }
}