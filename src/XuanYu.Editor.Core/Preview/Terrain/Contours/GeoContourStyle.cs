namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoContourStyle(
    int IntervalMeters, float Opacity, float WidthMeters, float LiftMeters)
{
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
        return new(interval, opacity, width, Math.Clamp(width * 0.22f, 0.25f, 12f));
    }
}