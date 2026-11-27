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
}