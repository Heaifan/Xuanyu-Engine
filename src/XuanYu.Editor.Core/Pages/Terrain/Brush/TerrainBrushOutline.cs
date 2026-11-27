using System;
using System.Collections.Generic;
using System.Numerics;

namespace XuanYu.Editor.TerrainEditing;

public static class TerrainBrushOutline
{
    public readonly record struct Segment(Vector3 Start, Vector3 End);

    public static IReadOnlyList<Segment> Build(
        Vector2 center, float radiusMeters, float sampleSpacingMeters,
        Func<Vector2, float?> sampleSurface)
    {
        // 采样密度跟随地形精度；限制上限以保持预览轻量。
        double count = Math.Tau * radiusMeters / sampleSpacingMeters;
        int steps = (int)Math.Clamp(Math.Ceiling(count), 64d, 512d);
        var result = new List<Segment>(steps);
        Vector3? first = Sample(center, radiusMeters, 0f, sampleSurface);
        Vector3? previous = first;
        for (int i = 1; i <= steps; i++)
        {
            Vector3? next = i == steps ? first : Sample(center, radiusMeters,
                MathF.Tau * i / steps, sampleSurface);
            if (previous is Vector3 a && next is Vector3 b)
                result.Add(new Segment(a, b));
            previous = next;
        }
        return result;
    }

    private static Vector3? Sample(Vector2 center, float radius, float angle,
        Func<Vector2, float?> sampleSurface)
    {
        Vector2 xy = center + radius * new Vector2(
            MathF.Cos(angle), MathF.Sin(angle));
        float? height = sampleSurface(xy);
        return height is float z ? new Vector3(xy, z) : null;
    }
}