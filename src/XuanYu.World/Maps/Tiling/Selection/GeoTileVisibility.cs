namespace XuanYu.World.Maps;

public static class GeoTileVisibility
{
    public static bool IntersectsView(GeoTileView view, GeoTileSphere sphere)
    {
        double dx = sphere.Center.X - view.Position.X;
        double dy = sphere.Center.Y - view.Position.Y;
        double dz = sphere.Center.Z - view.Position.Z;
        double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (distance <= sphere.RadiusMeters) return true;

        double dot = (dx * view.ForwardX + dy * view.ForwardY +
            dz * view.ForwardZ) / distance;
        double angle = Math.Acos(Math.Clamp(dot, -1d, 1d));
        double sphereHalfAngle = Math.Asin(Math.Min(1d, sphere.RadiusMeters / distance));
        return angle <= view.ConeHalfAngleRadians + sphereHalfAngle;
    }
}