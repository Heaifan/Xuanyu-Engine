using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainSeamWire
{
    private const int MaxTriangles = 384;

    public static RenderMesh? Build(RenderMesh source)
    {
        ReadOnlySpan<float> data = source.Vertices.Span;
        var output = new List<float>(Math.Min(source.TriangleCount, MaxTriangles) * 126);
        int step = Math.Max(1, (source.TriangleCount + MaxTriangles - 1) / MaxTriangles);
        Vector3 ink = new(0.07f, 0.11f, 0.18f);
        for (int t = 0; t < source.TriangleCount; t += step)
        {
            int i = t * 21;
            Vector3 a = Read(data, i), b = Read(data, i + 7), c = Read(data, i + 14);
            AddEdge(output, a, b, ink, 0.7f, 0.7f);
            AddEdge(output, b, c, ink, 0.7f, 0.7f);
            AddEdge(output, c, a, ink, 0.7f, 0.7f);
        }
        return output.Count == 0 ? null :
            new RenderMesh(source.AssetId + "/g06-light-wire", output.ToArray());
    }

    internal static Vector3 Read(ReadOnlySpan<float> data, int i) =>
        new(data[i], data[i + 1], data[i + 2]);

    internal static void AddEdge(List<float> output, Vector3 a, Vector3 b,
        Vector3 ink, float width, float lift)
    {
        Vector3 direction = b - a;
        Vector3 side = new(-direction.Y, direction.X, 0f);
        if (side.LengthSquared() < 1e-8f) side = Vector3.UnitX;
        side = Vector3.Normalize(side) * (width * 0.5f);
        Vector3 up = new(0f, 0f, lift);
        Vector3 a0 = a - side + up, a1 = a + side + up;
        Vector3 b0 = b - side + up, b1 = b + side + up;
        Write(output, a0, ink); Write(output, b0, ink); Write(output, b1, ink);
        Write(output, a0, ink); Write(output, b1, ink); Write(output, a1, ink);
    }

    private static void Write(List<float> output, Vector3 point, Vector3 ink)
    {
        output.Add(point.X); output.Add(point.Y); output.Add(point.Z);
        output.Add(ink.X); output.Add(ink.Y); output.Add(ink.Z);
        output.Add(1f);
    }
}