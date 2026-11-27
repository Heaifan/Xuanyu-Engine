using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public static class TerrainThermalStamp
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
        int width = x1 - x0 + 1, length = y1 - y0 + 1;
        var heights = new float[width * length];
        var weights = new float[heights.Length];
        for (int y = 0; y < length; y++)
        for (int x = 0; x < width; x++)
        {
            int i = y * width + x;
            heights[i] = field.GetHeight(x + x0, y + y0);
            weights[i] = brush.WeightAt(Vector2.Distance(center,
                new((x + x0) * sx, (y + y0) * sy)));
        }
        if (!TerrainThermalRelaxation.Step(heights, new float[heights.Length],
            weights, width, sx, sy, strength)) return 0;
        int changed = 0;
        for (int i = 0; i < heights.Length; i++)
        {
            int x = x0 + i % width, y = y0 + i / width;
            if (field.GetHeight(x, y) == heights[i]) continue;
            field.SetHeight(x, y, heights[i]);
            changed++;
        }
        return changed;
    }

    private static int Bound(float meters, float spacing, int samples) =>
        (int)Math.Clamp(MathF.Floor(meters / spacing), 0f, samples - 1f);
}