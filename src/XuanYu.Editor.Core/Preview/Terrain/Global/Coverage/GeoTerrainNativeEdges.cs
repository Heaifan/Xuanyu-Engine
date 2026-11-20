using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainNativeEdges
{
    // N, S, W, E followed by NW, NE, SW, SE. Refine a corner only if all four tiles agree.
    public static int Mask(GeoTileId id, Func<GeoTileId, bool> ready)
    {
        bool Has(int dx, int dy)
        {
            int x = id.X + dx, y = id.Y + dy;
            return (uint)x < (2u << id.Level) && (uint)y < (1u << id.Level) &&
                ready(new GeoTileId(id.Level, x, y));
        }
        bool north = Has(0, -1), south = Has(0, 1), west = Has(-1, 0), east = Has(1, 0);
        return (north ? 1 : 0) | (south ? 2 : 0) | (west ? 4 : 0) | (east ? 8 : 0) |
            (north && west && Has(-1, -1) ? 16 : 0) | (north && east && Has(1, -1) ? 32 : 0) |
            (south && west && Has(-1, 1) ? 64 : 0) | (south && east && Has(1, 1) ? 128 : 0);
    }

    public static GeoTerrainFootprintTile Refine(GeoTerrainFootprintTile tile, int mask,
        GeoTerrainFootprintGrid grid, NasademElevationSource source, EnuFrame anchor)
    {
        if (mask == 0) return tile;
        GeoTileBounds b = tile.Bounds;
        var edges = tile.Edges.ToArray();
        var inherited = tile.CoarseEdges.ToArray();
        for (int side = 0; side < edges.Length; side++)
        {
            if ((mask & 1 << side) == 0) continue;
            bool horizontal = side < 2;
            double fixedValue = side switch
            {
                0 => b.NorthLatitudeDegrees, 1 => b.SouthLatitudeDegrees,
                2 => b.WestLongitudeDegrees, _ => b.EastLongitudeDegrees
            };
            double min = horizontal ? b.WestLongitudeDegrees : b.SouthLatitudeDegrees;
            double max = horizontal ? b.EastLongitudeDegrees : b.NorthLatitudeDegrees;
            var knots = grid.Axis(tile.Id, min, max, horizontal, true, false, []);
            var vertices = new GeoTerrainEdgeVertex?[knots.Length];
            for (int i = 0; i < knots.Length; i++)
            {
                double latitude = horizontal ? fixedValue : knots[i];
                double longitude = horizontal ? knots[i] : fixedValue;
                if (i == 0 || i == knots.Length - 1)
                {
                    int corner = (latitude == b.NorthLatitudeDegrees ? 4 : 6) +
                        (longitude == b.EastLongitudeDegrees ? 1 : 0);
                    if ((mask & 1 << corner) == 0)
                    {
                        tile.TryBoundary(latitude, longitude, out vertices[i]);
                        continue;
                    }
                }
                vertices[i] = GeoTerrainFootprintEdge.Sample(source, anchor, latitude, longitude);
            }
            edges[side] = new GeoTerrainFootprintEdge(knots, vertices);
            inherited[side] = false;
        }
        return new GeoTerrainFootprintTile(tile.Id, b, edges, inherited, true);
    }
}
