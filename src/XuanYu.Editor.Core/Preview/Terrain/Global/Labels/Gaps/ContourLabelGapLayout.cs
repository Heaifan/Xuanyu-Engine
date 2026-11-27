using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal static class ContourLabelGapLayout
{
    public static RenderDraw[] Attach(IReadOnlyList<RenderDraw> draws,
        IReadOnlyList<GeoContourLabel> anchors, Matrix4x4 projection,
        GeoContourLabelVisibility visibility, Func<RenderDraw, GeoContourLabel, bool> canOwn,
        uint width, uint height, float scale, out GeoContourLabel[] visible,
        Func<int, float>? measure)
    {
        RenderDraw[] result = draws.ToArray();
        var labels = new List<GeoContourLabel>();
        if (anchors.Count == 0)
        {
            visible = [];
            return result;
        }
        var gaps = new List<Vector4>?[result.Length];
        visibility.Prepare(result, projection);
        foreach (GeoContourLabel anchor in anchors)
        {
            int index = Array.FindIndex(result, d =>
                ReferenceEquals(d.Mesh, anchor.Owner) && canOwn(d, anchor));
            if (index < 0 || !GeoContourLabelProjection.TryScreen(anchor,
                projection, width, height, out GeoContourLabel label)) continue;
            if (label.ContourId > 0 && labels.Any(old =>
                old.ContourId == label.ContourId)) continue;
            float radians = label.AngleDegrees * MathF.PI / 180f;
            float halfWidth = ((measure?.Invoke(label.ElevationMeters) ?? 64f) / 2f + 2f) * scale;
            if (labels.Any(old => MathF.Abs(old.X - label.X) <
                halfWidth * 2f + 8f && MathF.Abs(old.Y - label.Y) < 20f)) continue;
            if (!visibility.IsVisible(label, halfWidth, radians, width, height)) continue;
            uint packed = (uint)BitConverter.HalfToUInt16Bits((Half)radians) |
                ((uint)BitConverter.HalfToUInt16Bits((Half)halfWidth) << 16);
            (gaps[index] ??= []).Add(new Vector4(label.X, label.Y,
                BitConverter.UInt32BitsToSingle(packed), label.ElevationMeters));
            labels.Add(label);
        }
        for (int i = 0; i < result.Length; i++)
            if (gaps[i] is { Count: > 0 } entries)
                result[i] = result[i] with
                {
                    ContourLabelGap = entries[0],
                    ContourLabelGaps = entries.ToArray()
                };
        visible = labels.ToArray();
        return result;
    }
}