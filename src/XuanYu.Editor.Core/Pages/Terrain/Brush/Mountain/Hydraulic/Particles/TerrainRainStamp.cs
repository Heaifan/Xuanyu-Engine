using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public static class TerrainRainStamp
{
    public static int Apply(TerrainHeightField field, Vector2 center,
        TerrainBrushSettings brush, int droplets, int seed)
    {
        if (droplets == 0 || brush.Strength == 0f) return 0;
        float sx = field.SpacingXMeters, sy = field.SpacingYMeters;
        double r = brush.RadiusMeters;
        int x0 = Bound(Math.Floor((center.X - r) / sx) - 2, field.SamplesX);
        int x1 = Bound(Math.Ceiling((center.X + r) / sx) + 2, field.SamplesX);
        int y0 = Bound(Math.Floor((center.Y - r) / sy) - 2, field.SamplesY);
        int y1 = Bound(Math.Ceiling((center.Y + r) / sy) + 2, field.SamplesY);
        int width = x1 - x0 + 1, height = y1 - y0 + 1;
        if (width < 4 || height < 4) return 0;
        var heights = new float[width * height];
        var weights = new float[heights.Length];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int i = y * width + x;
            heights[i] = field.GetHeight(x + x0, y + y0);
            // 核心算法保留最外层样本；额外两格提供局部计算边界。
            if (x == 0 || y == 0 || x == width - 1 || y == height - 1) continue;
            weights[i] = brush.WeightAt(Vector2.Distance(center,
                new Vector2((x + x0) * sx, (y + y0) * sy)));
        }
        TerrainRainErosion.Apply(heights, width, sx, sy, droplets, brush.Strength, seed, weights);
        int changed = 0;
        for (int i = 0; i < heights.Length; i++)
        {
            int x = x0 + i % width, y = y0 + i / width;
            if (heights[i] == field.GetHeight(x, y)) continue;
            field.SetHeight(x, y, heights[i]);
            changed++;
        }
        return changed;
    }

    private static int Bound(double index, int samples) =>
        (int)Math.Clamp(index, 0d, samples - 1d);
}