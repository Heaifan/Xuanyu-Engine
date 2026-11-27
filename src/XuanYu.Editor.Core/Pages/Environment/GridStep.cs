namespace XuanYu.Editor.Preview;

public static class GridStep
{
    public static double FromDistance(double distanceMeters)
    {
        double wanted = Math.Clamp(distanceMeters / 10d, 1d, 10_000_000d);
        double decade = Math.Pow(10d, Math.Floor(Math.Log10(wanted)));
        double ratio = wanted / decade;
        double multiple = ratio <= 1d ? 1d : ratio <= 2d ? 2d : ratio <= 5d ? 5d : 10d;
        return multiple * decade;
    }
    public static (double Step, double Next, float Blend) ForViewport(
        double distanceMeters, uint height)
    {
        double wanted = Math.Clamp(30d * 2d * distanceMeters * Math.Tan(Math.PI / 6d) /
            Math.Max(1u, height), 1d, 10_000_000d);
        double decade = Math.Pow(10d, Math.Floor(Math.Log10(wanted)));
        double ratio = wanted / decade;
        double lower = ratio < 2d ? 1d : ratio < 5d ? 2d : 5d;
        double upper = lower == 1d ? 2d : lower == 2d ? 5d : 10d;
        double t = Math.Clamp(Math.Log(ratio / lower) / Math.Log(upper / lower), 0d, 1d);
        return (lower * decade, upper * decade, (float)(t * t * (3d - 2d * t)));
    }
}