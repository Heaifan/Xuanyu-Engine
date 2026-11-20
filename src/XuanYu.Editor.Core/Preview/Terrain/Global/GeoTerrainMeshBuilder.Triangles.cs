using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainMeshBuilder
{
    private static void AddTriangle(List<float> output,
        Vector3[] points, Vector4[] colors, bool[] valid,
        int a, int b, int c)
    {
        if (!valid[a] || !valid[b] || !valid[c]) return;
        AddVertex(output, points[a], colors[a]);
        AddVertex(output, points[b], colors[b]);
        AddVertex(output, points[c], colors[c]);
    }

    private static void AddVertex(List<float> output, Vector3 p, Vector4 color)
    {
        output.Add(p.X); output.Add(p.Y); output.Add(p.Z);
        output.Add(color.X); output.Add(color.Y); output.Add(color.Z);
        output.Add(color.W);
    }
}