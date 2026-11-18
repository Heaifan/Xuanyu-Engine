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
        double[] xs = Axis(west, east, tile.WestLongitude,
            ExtraLongitudes(bounds, edges));
        double[] ys = Axis(south, north, tile.SouthLatitude,
            ExtraLatitudes(bounds, edges));
        int width = xs.Length, height = ys.Length;
        var points = new Vector3[width * height];
        var colors = new Vector3[width * height];
        var valid = new bool[width * height];
        var blocks = new Dictionary<(int, int), NasademSampleBlock>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                double? h = ReadHeight(tile, source, xs[x], ys[y], blocks);
                if (h is not { } elevation) continue;
                EnuPosition p = anchor.ToEnu(Wgs84.ToEcef(
                    new GeoCoordinate(ys[y], xs[x], 0d)));
                Vector3 position = new((float)p.East, (float)p.North,
                    (float)(p.Up + elevation));
                int index = y * width + x;
                Vector3? border = BorderPosition(position, bounds, edges,
                    xs[x], ys[y], x, y, width, height);
                if (border is not { } finalPosition) continue;
                points[index] = finalPosition;
                float t = Math.Clamp((float)((elevation + 100d) / 3000d), 0f, 1f);
                colors[index] = Vector3.Lerp(new(0.21f, 0.35f, 0.24f),
                    new(0.66f, 0.64f, 0.58f), t);
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
            new RenderMesh($"terrain/g04/{tile.TileId}/{id}/{neighbors}",
                vertices.ToArray());
    }
}