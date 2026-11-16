namespace XuanYu.World.Maps;

public static class GeoTileSse
{
    public static double EstimatePixels(GeoTileView view, GeoTileSphere sphere,
        int level, double rootErrorMeters)
    {
        double distance = Math.Max(1d,
            sphere.DistanceTo(view.Position) - sphere.RadiusMeters);
        double estimatedErrorMeters = Math.ScaleB(rootErrorMeters, -level);
        return estimatedErrorMeters * view.ProjectionScalePixels / distance;
    }
}