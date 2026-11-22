using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoContourLabel(
    float X, float Y, int ElevationMeters, float AngleDegrees,
    float East, float North, float Up, float TangentEast, float TangentNorth,
    int South, int West)
{
    public RenderMesh? Owner { get; init; }
    public float TangentUp { get; init; }
    public int ContourId { get; init; }
}

internal static class GeoContourLabelLayout
{
    public static GeoContourLabel[] Select(IReadOnlyList<GeoContourLabel> candidates,
        uint width, uint height)
    {
        var ordered = candidates.OrderBy(point =>
        {
            float dx = (point.X - width / 2f) / width;
            float dy = (point.Y - height / 2f) / height;
            return dx * dx + dy * dy + MathF.Abs(point.AngleDegrees) * 0.00015f;
        });
        var accepted = new List<GeoContourLabel>();
        var counts = new Dictionary<int, int>();
        foreach (var point in ordered)
        {
            counts.TryGetValue(point.ContourId, out int count);
            if (point.ContourId > 0 && count >= 3) continue;
            if (accepted.Any(old => old.ContourId == point.ContourId &&
                MathF.Abs(old.X - point.X) < 80f &&
                MathF.Abs(old.Y - point.Y) < 30f)) continue;
            accepted.Add(point);
            if (point.ContourId > 0) counts[point.ContourId] = count + 1;
        }
        return accepted.ToArray();
    }
}