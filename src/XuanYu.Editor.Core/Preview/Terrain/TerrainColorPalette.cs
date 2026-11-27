using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

internal static class TerrainColorPalette
{
    private static readonly Vector3 Grass = new(0.25f, 0.54f, 0.27f);
    private static readonly Vector3 Soil = new(0.60f, 0.45f, 0.25f);
    private static readonly Vector3 Rock = new(0.49f, 0.47f, 0.44f);

    private static readonly double[] ElevationMeters =
        [0d, 200d, 800d, 1600d, 2800d, 4200d, 6000d];
    private static readonly Vector3[] ElevationColors =
    [
        Rgb(0x5CBB6F), Rgb(0x72C96A), Rgb(0xA8D96D),
        Rgb(0xD5DF83), Rgb(0xEDCF79), Rgb(0xBF9369), Rgb(0xA9A5A0)
    ];

    public static IReadOnlyList<Vector4> GpuElevation { get; } = Array.AsReadOnly(
        ElevationColors.Select((color, i) => new Vector4(color, (float)ElevationMeters[i])).ToArray());

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

    public static Vector3 Elevation(double elevationMeters)
    {
        if (elevationMeters <= ElevationMeters[0]) return ElevationColors[0];
        for (int i = 1; i < ElevationMeters.Length; i++)
        {
            if (elevationMeters > ElevationMeters[i]) continue;
            float t = (float)((elevationMeters - ElevationMeters[i - 1]) /
                (ElevationMeters[i] - ElevationMeters[i - 1]));
            return Vector3.Lerp(ElevationColors[i - 1], ElevationColors[i], t);
        }
        return ElevationColors[^1];
    }

    private static Vector3 Rgb(int hex) => new(
        ((hex >> 16) & 255) / 255f,
        ((hex >> 8) & 255) / 255f,
        (hex & 255) / 255f);

    private static float Smooth(float min, float max, float value)
    {
        float t = Math.Clamp((value - min) / (max - min), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}