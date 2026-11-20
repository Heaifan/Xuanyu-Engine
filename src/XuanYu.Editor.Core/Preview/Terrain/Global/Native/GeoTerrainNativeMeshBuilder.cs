using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainNativeMeshBuilder
{
    public static RenderMesh? Build(NasademTile tile, GeoTileId id,
        EnuFrame anchor, NasademElevationSource source,
        GeoTileBounds coverage, GeoTerrainEdgeNeighbors neighbors)
    {
        ArgumentNullException.ThrowIfNull(tile);
        return Build(tile.TileId, id, anchor, source, coverage, neighbors);
    }

    internal static RenderMesh? Build(string sourceId, GeoTileId id,
        EnuFrame anchor, NasademElevationSource source,
        GeoTileBounds coverage, GeoTerrainEdgeNeighbors neighbors)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(source);
        GeoTileBounds area = GeographicTileScheme.Bounds(id);
        double south = Math.Max(area.SouthLatitudeDegrees, coverage.SouthLatitudeDegrees);
        double north = Math.Min(area.NorthLatitudeDegrees, coverage.NorthLatitudeDegrees);
        double west = Math.Max(area.WestLongitudeDegrees, coverage.WestLongitudeDegrees);
        double east = Math.Min(area.EastLongitudeDegrees, coverage.EastLongitudeDegrees);
        if (south >= north || west >= east) return null;
        var bounds = new GeoTileBounds(south, west, north, east);
        NativeEdgeProfile[] edges = EdgeProfiles(bounds, neighbors, source, anchor, coverage);
        double[] xs = Axis(west, east, ExtraLongitudes(bounds, edges));
        double[] ys = Axis(south, north, ExtraLatitudes(bounds, edges));
        int width = xs.Length, height = ys.Length;
        var points = new Vector3[width * height];
        var colors = new Vector4[width * height];
        var valid = new bool[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                double? h = source.Sample(ys[y], xs[x]);
                if (h is not { } elevation) continue;
                EnuPosition p = anchor.ToEnu(Wgs84.ToEcef(
                    new GeoCoordinate(ys[y], xs[x], 0d)));
                Vector3 position = new((float)p.East, (float)p.North,
                    (float)(p.Up + elevation));
                int index = y * width + x;
                var vertex = GeoTerrainEdgeVertex.FromHeight(position, elevation);
                GeoTerrainEdgeVertex? border = BorderVertex(vertex, bounds, edges,
                    xs[x], ys[y], x, y, width, height);
                if (border is not { } finalVertex) continue;
                points[index] = finalVertex.Position;
                colors[index] = new(finalVertex.Color, finalVertex.Elevation);
                valid[index] = true;
            }
        var vertices = new List<float>();
        for (int y = 0; y < height - 1; y++)
            for (int x = 0; x < width - 1; x++)
            {
                int a = y * width + x, b = a + 1;
                int c = a + width, d = c + 1;
                Triangle(vertices, points, colors, valid, a, b, d);
                Triangle(vertices, points, colors, valid, a, d, c);
            }
        return vertices.Count == 0 ? null :
            RenderMesh.FromOpaqueTerrain($"terrain/g04/{sourceId}/{id}/{neighbors}",
                System.Runtime.InteropServices.CollectionsMarshal.AsSpan(vertices));
    }
}