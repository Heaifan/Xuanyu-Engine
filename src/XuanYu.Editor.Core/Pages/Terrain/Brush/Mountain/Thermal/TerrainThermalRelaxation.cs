using System.Numerics;

namespace XuanYu.Editor.TerrainEditing;

internal static class TerrainThermalRelaxation
{
    private static readonly float Talus = MathF.Tan(35f * MathF.PI / 180f);

    public static bool Step(float[] heights, float[] delta, float[]? weights,
        int width, float sx, float sy, float strength)
    {
        Array.Clear(delta);
        int length = heights.Length / width;
        float diagonal = new Vector2(sx, sy).Length();
        for (int y = 0; y < length; y++)
        for (int x = 0; x < width; x++)
        {
            int i = y * width + x;
            if (weights is not null && weights[i] == 0f) continue;
            if (x + 1 < width) Transfer(i, i + 1, sx);
            if (y + 1 >= length) continue;
            Transfer(i, i + width, sy);
            if (x + 1 < width) Transfer(i, i + width + 1, diagonal);
            if (x > 0) Transfer(i, i + width - 1, diagonal);
        }
        bool changed = false;
        for (int i = 0; i < heights.Length; i++)
        {
            float next = heights[i] + delta[i];
            if (next == heights[i]) continue;
            heights[i] = next;
            changed = true;
        }
        return changed;

        void Transfer(int i, int j, float distance)
        {
            float weight = weights is null ? 1f : MathF.Min(weights[i], weights[j]);
            if (weight == 0f) return;
            float difference = heights[i] - heights[j];
            float excess = MathF.Abs(difference) - Talus * distance;
            if (excess <= 0f) return;
            // 每点最多八条边，每边最多转移超额的 1/16。
            float amount = MathF.CopySign(excess * weight * strength / 16f, difference);
            delta[i] -= amount;
            delta[j] += amount;
        }
    }
}