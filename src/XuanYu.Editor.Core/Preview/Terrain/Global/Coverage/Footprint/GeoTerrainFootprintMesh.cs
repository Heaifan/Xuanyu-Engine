using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainFootprintMesh
{
    public static RenderMesh? Build(GeoTerrainFootprintGrid grid, GeoTerrainFootprintTile tile,
        NasademElevationSource source, EnuFrame anchor, bool native, string sourceId)
    {
        GeoTileBounds b = tile.Bounds;
        var edges = tile.Edges;
        bool cut = !grid.IsFull(b);
        double[] xs = grid.Axis(tile.Id, b.WestLongitudeDegrees, b.EastLongitudeDegrees,
            true, native, cut, edges[0].Knots.Concat(edges[1].Knots));
        double[] ys = grid.Axis(tile.Id, b.SouthLatitudeDegrees, b.NorthLatitudeDegrees,
            false, native, cut, edges[2].Knots.Concat(edges[3].Knots));
        int width = xs.Length, height = ys.Length;
        var points = new GeoTerrainEdgeVertex?[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (tile.TryBoundary(ys[y], xs[x], out GeoTerrainEdgeVertex? edge))
                    points[y * width + x] = edge;
                else if (GeoTerrainFootprintEdge.Sample(source, anchor, ys[y], xs[x]) is { } point)
                    points[y * width + x] = native
                        ? GeoTerrainFootprintTransition.Blend(tile, ys[y], xs[x], point)
                        : point;
            }
        var data = new List<float>();
        for (int y = 0; y < height - 1; y++)
            for (int x = 0; x < width - 1; x++)
            {
                // 缺失区域只裁剪格元，从不生成海平面占位或跨洞三角形。
                if (cut && !grid.Covers((ys[y] + ys[y + 1]) / 2d, (xs[x] + xs[x + 1]) / 2d))
                    continue;
                int a = y * width + x, b0 = a + 1, c = a + width, d = c + 1;
                Triangle(data, points[a], points[b0], points[d]);
                Triangle(data, points[a], points[d], points[c]);
            }
        if (data.Count == 0) return null;
        string asset = native ? $"terrain/g04/{sourceId}/{tile.Id}/footprint" : $"terrain/g02/{tile.Id}";
        return RenderMesh.FromOpaqueTerrain(asset, System.Runtime.InteropServices.CollectionsMarshal.AsSpan(data));
    }

    private static void Triangle(List<float> output, GeoTerrainEdgeVertex? a,
        GeoTerrainEdgeVertex? b, GeoTerrainEdgeVertex? c)
    {
        if (a is not { } va || b is not { } vb || c is not { } vc) return;
        Write(output, va); Write(output, vb); Write(output, vc);
    }

    private static void Write(List<float> output, GeoTerrainEdgeVertex v)
    {
        output.Add(v.Position.X); output.Add(v.Position.Y); output.Add(v.Position.Z);
        output.Add(v.Color.X); output.Add(v.Color.Y); output.Add(v.Color.Z); output.Add(v.Elevation);
    }
}
