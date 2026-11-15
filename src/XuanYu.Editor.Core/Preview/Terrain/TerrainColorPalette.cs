
using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

internal static class TerrainColorPalette
{
    private static readonly Vector3 Grass = new(0.25f, 0.54f, 0.27f);
    private static readonly Vector3 Soil = new(0.60f, 0.45f, 0.25f);
    private static readonly Vector3 Rock = new(0.49f, 0.47f, 0.44f);

    private static readonly Vector3 Low = new(0.13f, 0.33f, 0.20f);
    private static readonly Vector3 LowMid = new(0.53f, 0.72f, 0.36f);
    private static readonly Vector3 Mid = new(0.83f, 0.73f, 0.34f);
    private static readonly Vector3 HighMid = new(0.58f, 0.42f, 0.30f);
    private static readonly Vector3 High = new(0.85f, 0.86f, 0.83f);

    public static Vector3 Standard(Vector3 normal, float height)
    {
        float degrees = MathF.Acos(Math.Clamp(normal.Z, 0f, 1f))
            * (180f / MathF.PI);
        Vector3 color = Vector3.Lerp(Grass, Soil,
            Smooth(12f, 32f, degrees));
        color = Vector3.Lerp(color, Rock,
            Smooth(30f, 52f, degrees));
        return color * (0.90f + height * 0.10f);
    }

    public static Vector3 Elevation(float height)
    {
        float step = Math.Clamp(height, 0f, 1f) * 4f;
        if (step < 1f)
            return Vector3.Lerp(Low, LowMid, step);
        if (step < 2f)
            return Vector3.Lerp(LowMid, Mid, step - 1f);
        if (step < 3f)
            return Vector3.Lerp(Mid, HighMid, step - 2f);
        return Vector3.Lerp(HighMid, High, step - 3f);
    }

    private static float Smooth(float min, float max, float value)
    {
        float t = Math.Clamp((value - min) / (max - min), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}
