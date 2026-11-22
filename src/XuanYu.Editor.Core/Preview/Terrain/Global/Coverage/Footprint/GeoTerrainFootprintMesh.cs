using System.Runtime.InteropServices;
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
        var geographic = new List<double>((width - 1) * (height - 1) * 12);
        for (int y = 0; y < height - 1; y++)
            for (int x = 0; x < width - 1; x++)
            {
                // 缺失区域只裁剪格元，从不生成海平面占位或跨洞三角形。
                if (cut && !grid.Covers((ys[y] + ys[y + 1]) / 2d, (xs[x] + xs[x + 1]) / 2d))
                    continue;
                int a = y * width + x, b0 = a + 1, c = a + width, d = c + 1;
                Triangle(a, b0, d);
                Triangle(a, d, c);
            }
        if (data.Count == 0) return null;
        string asset = native ? $"terrain/g04/{sourceId}/{tile.Id}/footprint" : $"terrain/g02/{tile.Id}";
        return RenderMesh.FromLitTerrain(asset, CollectionsMarshal.AsSpan(data),
            CollectionsMarshal.AsSpan(geographic));

        void Triangle(int a, int b, int c)
        {
            if (points[a] is not { } va || points[b] is not { } vb || points[c] is not { } vc) return;
            Write(a, va); Write(b, vb); Write(c, vc);
        }

        void Write(int index, GeoTerrainEdgeVertex vertex)
        {
            vertex.AppendTo(data);
            int x = index % width, y = index / width;
            // 共边点可能是 ENU 线性插值结果，不能以名义格点替换原有 UV 反算。
            bool boundary = x == 0 || x == width - 1 || y == 0 || y == height - 1;
            geographic.Add(boundary ? double.NaN : xs[x]);
            geographic.Add(boundary ? double.NaN : ys[y]);
        }
    }
}