using System;

namespace XuanYu.Editor.TerrainEditing;

public sealed class TerrainBrushSettings
{
    public float RadiusMeters { get; }
    public float Strength { get; }
    public float Falloff { get; }

    public TerrainBrushSettings(float radiusMeters, float strength, float falloff)
    {
        if (!float.IsFinite(radiusMeters) || radiusMeters <= 0f)
            throw new ArgumentOutOfRangeException(nameof(radiusMeters));
        if (!float.IsFinite(strength) || strength < 0f || strength > 1f)
            throw new ArgumentOutOfRangeException(nameof(strength));
        if (!float.IsFinite(falloff) || falloff < 0f || falloff > 1f)
            throw new ArgumentOutOfRangeException(nameof(falloff));

        RadiusMeters = radiusMeters;
        Strength = strength;
        Falloff = falloff;
    }

    public float HalfWeightRadius => RadiusMeters * (1f - Falloff * 0.5f);

    public float WeightAt(float distanceMeters)
    {
        float t = distanceMeters / RadiusMeters;
        if (t >= 1f) return 0f;
        float inner = 1f - Falloff;
        if (t <= inner) return 1f;
        float u = (t - inner) / Falloff;
        return 1f - u * u * (3f - 2f * u);
    }
}