using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public static class TerrainHeightStamp
{
    public static int Apply(TerrainHeightField field, Vector2 center,
        TerrainBrushSettings brush, bool lower) =>
        ApplyDelta(field, center, brush, (lower ? -1f : 1f) * brush.Strength);

    internal static int ApplyDelta(TerrainHeightField field, Vector2 center,
        TerrainBrushSettings brush, float delta) => delta == 0f ? 0 :
        Visit(field, center, brush, (x, y, weight) => field.GetHeight(x, y) + delta * weight);

    internal static int Visit(TerrainHeightField field, Vector2 center,
        TerrainBrushSettings brush, Func<int, int, float, float> height)
    {
        float r = brush.RadiusMeters;
        float sx = field.SpacingXMeters, sy = field.SpacingYMeters;
        int minX = Bound(center.X - r, sx, field.SamplesX);
        int maxX = Bound(center.X + r, sx, field.SamplesX);
        int minY = Bound(center.Y - r, sy, field.SamplesY);
        int maxY = Bound(center.Y + r, sy, field.SamplesY);
        int changed = 0;
        for (int y = minY; y <= maxY; y++)
        for (int x = minX; x <= maxX; x++)
        {
            float weight = brush.WeightAt(Vector2.Distance(center, new(x * sx, y * sy)));
            if (weight == 0f) continue;
            float after = height(x, y, weight);
            if (field.GetHeight(x, y) == after) continue;
            field.SetHeight(x, y, after);
            changed++;
        }
        return changed;
    }

    private static int Bound(float meters, float spacing, int samples) =>
        (int)Math.Clamp(MathF.Floor(meters / spacing), 0f, samples - 1f);
}