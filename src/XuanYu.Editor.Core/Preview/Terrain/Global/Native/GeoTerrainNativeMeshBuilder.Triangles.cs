using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainNativeMeshBuilder
{
    private static void Triangle(List<float> output, Vector3[] positions,
        Vector4[] colors, bool[] valid, int a, int b, int c)
    {
        if (!valid[a] || !valid[b] || !valid[c]) return;
        Write(output, positions[a], colors[a]);
        Write(output, positions[b], colors[b]);
        Write(output, positions[c], colors[c]);
    }

    private static void Write(List<float> output, Vector3 p, Vector4 color)
    {
        output.Add(p.X); output.Add(p.Y); output.Add(p.Z);
        output.Add(color.X); output.Add(color.Y); output.Add(color.Z);
        output.Add(color.W);
    }
}