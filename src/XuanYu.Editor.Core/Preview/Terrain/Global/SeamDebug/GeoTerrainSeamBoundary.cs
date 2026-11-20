using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainSeamBoundary
{
    public const int MaxTriangles = 1500;

    // 必须完整计数一块网格的三角形；只抽样会把内部边误判为开边。
    public static RenderMesh? Build(RenderMesh source)
    {
        if (source.TriangleCount > MaxTriangles) return null;
        ReadOnlySpan<float> data = source.Vertices.Span;
        var counts = new Dictionary<(Vector3, Vector3), int>();
        for (int i = 0; i < data.Length; i += 21)
        {
            Vector3 a = GeoTerrainSeamWire.Read(data, i);
            Vector3 b = GeoTerrainSeamWire.Read(data, i + 7);
            Vector3 c = GeoTerrainSeamWire.Read(data, i + 14);
            Count(counts, a, b); Count(counts, b, c); Count(counts, c, a);
        }
        var output = new List<float>();
        Vector3 ink = new(0.9f, 0.07f, 0.6f);
        foreach (var (edge, count) in counts)
            if (count == 1)
                GeoTerrainSeamWire.AddEdge(output, edge.Item1, edge.Item2,
                    ink, 1.8f, 1.2f);
        return output.Count == 0 ? null :
            new RenderMesh(source.AssetId + "/g06-light-boundary", output.ToArray());
    }

    private static void Count(Dictionary<(Vector3, Vector3), int> counts,
        Vector3 a, Vector3 b)
    {
        var edge = Ordered(a, b);
        counts[edge] = counts.GetValueOrDefault(edge) + 1;
    }

    private static (Vector3, Vector3) Ordered(Vector3 a, Vector3 b)
    {
        if (a.X != b.X) return a.X < b.X ? (a, b) : (b, a);
        if (a.Y != b.Y) return a.Y < b.Y ? (a, b) : (b, a);
        return a.Z <= b.Z ? (a, b) : (b, a);
    }
}