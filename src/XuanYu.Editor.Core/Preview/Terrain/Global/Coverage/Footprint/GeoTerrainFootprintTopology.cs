using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainFootprintTopology
{
    public static Dictionary<GeoTileId, GeoTerrainFootprintTile> Build(
        GeoTerrainFootprintGrid grid, NasademElevationSource source, EnuFrame anchor,
        IEnumerable<GeoTileId> visible)
    {
        var result = new Dictionary<GeoTileId, GeoTerrainFootprintTile>();
        var ready = new List<GeoTerrainFootprintTile>();
        // 先建粗边，细边及 T 形交点直接继承已生成的真实折线。
        foreach (GeoTileId id in visible.OrderBy(id => id.Level).ThenBy(id => id.Y).ThenBy(id => id.X))
        {
            GeoTerrainFootprintTile tile = Create(grid, source.CoarseFor(id.Level), anchor, id, ready);
            result.Add(id, tile);
            ready.Add(tile);
        }
        return result;
    }

    private static GeoTerrainFootprintTile Create(GeoTerrainFootprintGrid grid,
        NasademElevationSource source, EnuFrame anchor, GeoTileId id,
        List<GeoTerrainFootprintTile> ready)
    {
        GeoTileBounds b = grid.Clip(id);
        var edges = new GeoTerrainFootprintEdge[4];
        bool adjusted = false;
        for (int side = 0; side < 4; side++)
        {
            GeoTerrainFootprintTile? coarse = ready.FirstOrDefault(t =>
                t.Id.Level < id.Level && t.Touches(b, side));
            if (coarse is not null)
            {
                edges[side] = coarse.Edges[side ^ 1];
                adjusted = true;
                continue;
            }
            bool horizontal = side < 2;
            double fixedValue = side switch
            {
                0 => b.NorthLatitudeDegrees, 1 => b.SouthLatitudeDegrees,
                2 => b.WestLongitudeDegrees, _ => b.EastLongitudeDegrees
            };
            double min = horizontal ? b.WestLongitudeDegrees : b.SouthLatitudeDegrees;
            double max = horizontal ? b.EastLongitudeDegrees : b.NorthLatitudeDegrees;
            double[] knots = grid.Axis(id, min, max, horizontal, false, false,
                grid.EdgeCuts(min, max, fixedValue, horizontal));
            var vertices = new GeoTerrainEdgeVertex?[knots.Length];
            for (int i = 0; i < knots.Length; i++)
            {
                double lat = horizontal ? fixedValue : knots[i];
                double lon = horizontal ? knots[i] : fixedValue;
                GeoTerrainEdgeVertex? raw = GeoTerrainFootprintEdge.Sample(source, anchor, lat, lon);
                vertices[i] = i == 0 || i == knots.Length - 1 ? Corner(ready, lat, lon, raw) : raw;
                adjusted |= vertices[i] != raw;
            }
            edges[side] = new GeoTerrainFootprintEdge(knots, vertices);
        }
        return new GeoTerrainFootprintTile(id, b, edges, adjusted);
    }

    private static GeoTerrainEdgeVertex? Corner(List<GeoTerrainFootprintTile> ready,
        double lat, double lon, GeoTerrainEdgeVertex? raw)
    {
        foreach (GeoTerrainFootprintTile tile in ready)
            if (tile.TryBoundary(lat, lon, out GeoTerrainEdgeVertex? vertex)) return vertex;
        return raw;
    }
}