using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainSeamAudit
{
    private const int MaxSamples = 1024;

    public static string Describe(GeoTileId id, RenderMesh mesh)
    {
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        int stride = Math.Max(1, (mesh.TriangleCount + MaxSamples - 1) / MaxSamples);
        double widest = 0d, steepest = 0d;
        int widthIndex = -1, slopeIndex = -1, sampled = 0;
        for (int t = 0; t < mesh.TriangleCount; t += stride)
        {
            sampled++;
            int i = t * 21;
            for (int edge = 0; edge < 3; edge++)
            {
                int a = i + edge * 7, b = i + ((edge + 1) % 3) * 7;
                double dx = (double)data[a] - data[b];
                double dy = (double)data[a + 1] - data[b + 1];
                double dz = Math.Abs((double)data[a + 2] - data[b + 2]);
                double horizontal = Math.Sqrt(dx * dx + dy * dy);
                double slope = dz / Math.Max(horizontal, 0.01d);
                if (horizontal > widest) { widest = horizontal; widthIndex = t; }
                if (slope > steepest) { steepest = slope; slopeIndex = t; }
            }
        }
        return $"[G06/轻诊断] 瓦片={id} 网格={mesh.AssetId} " +
            $"三角形={mesh.TriangleCount} 抽样={sampled} " +
            $"最大跨度(样本)={widest:F1}m {Triangle(data, widthIndex)} " +
            $"最大坡比(样本)={steepest:F2} {Triangle(data, slopeIndex)}";
    }

    private static string Triangle(ReadOnlySpan<float> data, int triangle)
    {
        if (triangle < 0) return "三角形=无";
        int i = triangle * 21;
        Vector3 a = GeoTerrainSeamWire.Read(data, i);
        Vector3 b = GeoTerrainSeamWire.Read(data, i + 7);
        Vector3 c = GeoTerrainSeamWire.Read(data, i + 14);
        return $"三角号={triangle} A={Point(a)} B={Point(b)} C={Point(c)}";
    }

    private static string Point(Vector3 p) =>
        $"({p.X:F1},{p.Y:F1},{p.Z:F1})";
}