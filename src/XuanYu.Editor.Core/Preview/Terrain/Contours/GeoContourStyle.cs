namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoContourStyle(
    int IntervalMeters, float Opacity, float WidthMeters, float LiftMeters)
{
    public int NextIntervalMeters { get; init; } = IntervalMeters;
    public float IntervalBlend { get; init; }

    public float OpacityAtLod(int level) => Opacity *
        Math.Clamp((level - 7f) / 2f, 0f, 1f);

    public static GeoContourStyle AtDistance(double distanceMeters)
    {
        int interval;
        float opacity;
        if (distanceMeters < 2_500d) (interval, opacity) = (20, 0.30f);
        else if (distanceMeters < 7_500d) (interval, opacity) = (50, 0.28f);
        else if (distanceMeters < 20_000d) (interval, opacity) = (100, 0.26f);
        else if (distanceMeters < 60_000d) (interval, opacity) = (200, 0.24f);
        else if (distanceMeters < 2_000_000d) (interval, opacity) = (500, 0.22f);
        else (interval, opacity) = (1000, 0.18f);
        double raw = Math.Clamp(distanceMeters / 600d, 0.125d, 8192d);
        float width = (float)Math.Pow(2d, Math.Round(Math.Log2(raw)));
        var style = new GeoContourStyle(interval, opacity, width,
            Math.Clamp(width * 0.22f, 0.25f, 12f));
        (double threshold, int before, int after)[] transitions =
        [
            (2_500d, 20, 50), (7_500d, 50, 100), (20_000d, 100, 200),
            (60_000d, 200, 500), (2_000_000d, 500, 1000)
        ];
        return BlendAtDistance(style, distanceMeters, transitions);
    }

    public static GeoContourStyle AtLocalDistance(double distanceMeters)
    {
        GeoContourStyle style = AtDistance(distanceMeters);
        if (distanceMeters > 1650d) return style;
        int interval = distanceMeters < 100d ? 1 : distanceMeters < 300d ? 2 :
            distanceMeters < 750d ? 5 : distanceMeters < 1500d ? 10 : 20;
        style = style with { IntervalMeters = interval, NextIntervalMeters = interval };
        (double threshold, int before, int after)[] transitions =
        [ (100d, 1, 2), (300d, 2, 5), (750d, 5, 10), (1500d, 10, 20) ];
        return BlendAtDistance(style, distanceMeters, transitions);
    }

    private static GeoContourStyle BlendAtDistance(GeoContourStyle style,
        double distanceMeters, (double threshold, int before, int after)[] transitions)
    {
        foreach (var (threshold, before, after) in transitions)
        {
            double start = threshold * 0.9d, end = threshold * 1.1d;
            if (distanceMeters < start || distanceMeters > end) continue;
            float t = (float)Math.Clamp((distanceMeters - start) / (end - start), 0d, 1d);
            t = t * t * (3f - 2f * t);
            return style with { IntervalMeters = before,
                NextIntervalMeters = after, IntervalBlend = t };
        }
        return style with { NextIntervalMeters = style.IntervalMeters };
    }
}