namespace XuanYu.World.Maps;

public static class GeoUnifiedScreenError
{
    public static double Estimate(double errorMeters, int heightPixels,
        double distanceMeters, double verticalFovRadians)
    {
        if (!double.IsFinite(errorMeters) || errorMeters < 0) throw new ArgumentOutOfRangeException(nameof(errorMeters));
        if (heightPixels <= 0) throw new ArgumentOutOfRangeException(nameof(heightPixels));
        if (!double.IsFinite(distanceMeters) || distanceMeters < 0) throw new ArgumentOutOfRangeException(nameof(distanceMeters));
        if (!double.IsFinite(verticalFovRadians) || verticalFovRadians <= 0 || verticalFovRadians >= Math.PI)
            throw new ArgumentOutOfRangeException(nameof(verticalFovRadians));
        if (distanceMeters == 0) return double.PositiveInfinity;
        return errorMeters * heightPixels / (2 * distanceMeters * Math.Tan(verticalFovRadians / 2));
    }

    public static bool ShouldRefine(double pixels, bool wasRefined) =>
        pixels > (wasRefined ? 1.5 : 2.5);
}
