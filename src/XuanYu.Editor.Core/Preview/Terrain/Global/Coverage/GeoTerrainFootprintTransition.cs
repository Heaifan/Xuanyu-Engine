using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainFootprintTransition
{
    private const double MetersPerLatitudeDegree = 111_132d;
    private const double MetersPerLongitudeDegree = 111_320d;
    private const double Arcsecond = 1d / 3600d;

    public static GeoTerrainEdgeVertex Blend(GeoTerrainFootprintTile tile,
        double latitude, double longitude, GeoTerrainEdgeVertex point)
    {
        double nearest = double.PositiveInfinity;
        double width = 0d;
        float inheritedHeight = point.Elevation;
        for (int side = 0; side < tile.CoarseEdges.Length; side++)
        {
            if (!tile.CoarseEdges[side]) continue;
            GeoTerrainEdgeVertex? edge = tile.Edges[side].At(side < 2 ? longitude : latitude);
            if (edge is not { } boundary) continue;
            double dx = point.Position.X - boundary.Position.X;
            double dy = point.Position.Y - boundary.Position.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double spacing = side < 2 ? MetersPerLatitudeDegree * Arcsecond :
                MetersPerLongitudeDegree * Math.Cos(latitude * Math.PI / 180d) * Arcsecond;
            if (distance >= spacing || distance >= nearest) continue;
            nearest = distance;
            width = spacing;
            inheritedHeight = boundary.Elevation;
        }
        if (!double.IsFinite(nearest)) return point;
        float t = (float)Math.Clamp(nearest / width, 0d, 1d);
        float elevation = float.Lerp(inheritedHeight, point.Elevation, t);
        Vector3 position = new(point.Position.X, point.Position.Y,
            point.Position.Z - point.Elevation + elevation);
        return GeoTerrainEdgeVertex.FromHeight(position, elevation);
    }
}
