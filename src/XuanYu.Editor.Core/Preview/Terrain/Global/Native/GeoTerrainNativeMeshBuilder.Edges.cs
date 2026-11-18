using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainNativeMeshBuilder
{
    private sealed record NativeEdgeProfile(double[] Knots, Vector3?[] Positions)
    {
        public Vector3? At(double t)
        {
            int i = Array.BinarySearch(Knots, t);
            if (i >= 0) return Positions[i];
            i = Math.Clamp(~i - 1, 0, 15);
            float u = (float)((t - Knots[i]) / (Knots[i + 1] - Knots[i]));
            return Positions[i] is { } a && Positions[i + 1] is { } b
                ? Vector3.Lerp(a, b, u) : null;
        }
    }

    private static NativeEdgeProfile[] EdgeProfiles(GeoTileBounds bounds,
        GeoTerrainEdgeNeighbors neighbors, NasademElevationSource source,
        EnuFrame anchor, GeoTileBounds coverage)
    {
        var result = new NativeEdgeProfile[4];
        GeoTileId?[] ids = [neighbors.North, neighbors.South,
            neighbors.West, neighbors.East];
        for (int edge = 0; edge < 4; edge++)
        {
            bool horizontal = edge < 2;
            double[] knots = Enumerable.Range(0, 17).Select(i => i / 16d).ToArray();
            if (ids[edge] is { } adjacent)
            {
                GeoTileBounds area = GeographicTileScheme.Bounds(adjacent);
                var coarse = new GeoTileBounds(
                    Math.Max(area.SouthLatitudeDegrees, coverage.SouthLatitudeDegrees),
                    Math.Max(area.WestLongitudeDegrees, coverage.WestLongitudeDegrees),
                    Math.Min(area.NorthLatitudeDegrees, coverage.NorthLatitudeDegrees),
                    Math.Min(area.EastLongitudeDegrees, coverage.EastLongitudeDegrees));
                knots = GeoTerrainEdgeLayout.Fit(bounds, coarse, 16, horizontal);
            }
            var positions = new Vector3?[17];
            for (int i = 0; i <= 16; i++)
            {
                int row = edge == 0 ? 0 : edge == 1 ? 16 : i;
                int col = edge == 2 ? 0 : edge == 3 ? 16 : i;
                double latitude = bounds.NorthLatitudeDegrees -
                    (bounds.NorthLatitudeDegrees - bounds.SouthLatitudeDegrees) * row / 16d;
                double longitude = bounds.WestLongitudeDegrees +
                    (bounds.EastLongitudeDegrees - bounds.WestLongitudeDegrees) * col / 16d;
                if (source.Sample(latitude, longitude) is not { } elevation)
                    continue;
                EnuPosition p = anchor.ToEnu(Wgs84.ToEcef(
                    new GeoCoordinate(latitude, longitude, 0d)));
                var position = new Vector3((float)p.East,
                    (float)p.North, (float)(p.Up + elevation));
                positions[i] = GeoTerrainEdgeSnap.Align(position, row, col,
                    16, bounds, neighbors, source, anchor, coverage);
            }
            result[edge] = new NativeEdgeProfile(knots, positions);
        }
        return result;
    }

    private static IEnumerable<double> ExtraLongitudes(GeoTileBounds bounds,
        NativeEdgeProfile[] edges) =>
        edges.Take(2).SelectMany(edge => edge.Knots.Select(t =>
            bounds.WestLongitudeDegrees +
            (bounds.EastLongitudeDegrees - bounds.WestLongitudeDegrees) * t));

    private static IEnumerable<double> ExtraLatitudes(GeoTileBounds bounds,
        NativeEdgeProfile[] edges) =>
        edges.Skip(2).SelectMany(edge => edge.Knots.Select(t =>
            bounds.NorthLatitudeDegrees -
            (bounds.NorthLatitudeDegrees - bounds.SouthLatitudeDegrees) * t));

    private static Vector3? BorderPosition(Vector3 point, GeoTileBounds bounds,
        NativeEdgeProfile[] edges, double longitude, double latitude,
        int x, int y, int width, int height)
    {
        if (y == height - 1 || y == 0)
        {
            double t = (longitude - bounds.WestLongitudeDegrees) /
                (bounds.EastLongitudeDegrees - bounds.WestLongitudeDegrees);
            return edges[y == height - 1 ? 0 : 1].At(t);
        }
        if (x == 0 || x == width - 1)
        {
            double t = (bounds.NorthLatitudeDegrees - latitude) /
                (bounds.NorthLatitudeDegrees - bounds.SouthLatitudeDegrees);
            return edges[x == 0 ? 2 : 3].At(t);
        }
        return point;
    }
}