using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public static class TerrainSmoothStamp
{
    public static int Apply(TerrainHeightField field, Vector2 center,
        TerrainBrushSettings brush) => Apply(field, center, brush, brush.Strength);

    internal static int Apply(TerrainHeightField field, Vector2 center,
        TerrainBrushSettings brush, float strength)
    {
        if (strength == 0f) return 0;
        float r = brush.RadiusMeters;
        float sx = field.SpacingXMeters, sy = field.SpacingYMeters;
        int x0 = Bound(center.X - r, sx, field.SamplesX);
        int x1 = Bound(center.X + r, sx, field.SamplesX);
        int y0 = Bound(center.Y - r, sy, field.SamplesY);
        int y1 = Bound(center.Y + r, sy, field.SamplesY);
        int width = x1 - x0 + 1;
        var next = new float[width * (y1 - y0 + 1)];
        double wx = 1d / ((double)sx * sx), wy = 1d / ((double)sy * sy);
        // 所有目标值先计算，再统一写入，邻域读取不受遍历顺序影响。
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            float before = field.GetHeight(x, y);
            float weight = strength * brush.WeightAt(
                Vector2.Distance(center, new Vector2(x * sx, y * sy)));
            next[(y - y0) * width + x - x0] = weight == 0f ? before :
                SmoothPoint(field, x, y, before, wx, wy, weight);
        }
        int changed = 0;
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            float height = next[(y - y0) * width + x - x0];
            if (height == field.GetHeight(x, y)) continue;
            field.SetHeight(x, y, height);
            changed++;
        }
        return changed;
    }

    private static float SmoothPoint(TerrainHeightField field, int x, int y,
        float before, double wx, double wy, float weight)
    {
        // 边界采用自身高度补足缺失邻点；不绕到对侧，也不引入零高程。
        double left = x > 0 ? field.GetHeight(x - 1, y) : before;
        double right = x + 1 < field.SamplesX ? field.GetHeight(x + 1, y) : before;
        double down = y > 0 ? field.GetHeight(x, y - 1) : before;
        double up = y + 1 < field.SamplesY ? field.GetHeight(x, y + 1) : before;
        double delta = ((left - before + right - before) * wx +
            (down - before + up - before) * wy) / (2d * (wx + wy));
        // 保留一半中心权重，避免满强度时孤立尖峰在邻点间来回振荡。
        return (float)(before + delta * weight * 0.5d);
    }

    private static int Bound(float meters, float spacing, int samples) =>
        (int)Math.Clamp(MathF.Floor(meters / spacing), 0f, samples - 1f);
}