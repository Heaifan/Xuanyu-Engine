using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoContourLabel(
    float X, float Y, int ElevationMeters, float AngleDegrees,
    float East, float North, float Up, float TangentEast, float TangentNorth,
    int South, int West)
{
    public RenderMesh? Owner { get; init; }
    public float TangentUp { get; init; }
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
        foreach (var point in ordered)
        {
            if (accepted.Any(old => MathF.Abs(point.X - old.X) < 30f &&
                                    MathF.Abs(point.Y - old.Y) < 20f)) continue;
            accepted.Add(point);
            if (accepted.Count == 64) break;
        }
        return accepted.ToArray();
    }
}