namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainNativeMeshBuilder
{
    private static void Triangle(List<float> output,
        GeoTerrainEdgeVertex?[] points, int a, int b, int c)
    {
        if (points[a] is not { } va || points[b] is not { } vb || points[c] is not { } vc) return;
        va.AppendTo(output); vb.AppendTo(output); vc.AppendTo(output);
    }
}