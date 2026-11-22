using System.Numerics;
using System.Runtime.InteropServices;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainRawGeometry
{
    public static RenderMesh Refine(RenderMesh mesh, NasademElevationSource source)
    {
        var vertices = mesh.Vertices.Span;
        var heights = mesh.TerrainElevations.Span;
        var colors = mesh.TerrainShadedColors.Span;
        var geo = mesh.TerrainGeographicCoordinates.Span;
        var output = new List<float>(mesh.VertexCount * 10);
        var nodes = new Dictionary<(double Lon, double Lat), GeoTerrainEdgeVertex>();
        for (int i = 0; i < mesh.VertexCount; i++)
        {
            int p = i * 7, c = i * 3, g = i * 2;
            var vertex = new GeoTerrainEdgeVertex(new(vertices[p], vertices[p + 1], vertices[p + 2]),
                new(vertices[p + 3], vertices[p + 4], vertices[p + 5]), heights[i])
            { ShadedColor = new(colors[c], colors[c + 1], colors[c + 2]) };
            // NaN 是现有共边标记：保留实际共边位置、高程及颜色，不重采样。
            if (!double.IsNaN(geo[g]))
            {
                var key = (geo[g], geo[g + 1]);
                if (!nodes.TryGetValue(key, out var refined))
                {
                    refined = Sample(vertex, source, key.Item2, key.Item1);
                    nodes.Add(key, refined);
                }
                vertex = refined;
            }
            vertex.AppendTo(output);
        }
        return RenderMesh.FromLitTerrain(mesh.AssetId, CollectionsMarshal.AsSpan(output), geo);
    }

    private static GeoTerrainEdgeVertex Sample(GeoTerrainEdgeVertex old,
        NasademElevationSource source, double lat, double lon)
    {
        if (source.Sample(lat, lon) is not { } height) return old;
        Vector3 p = old.Position;
        p.Z = (float)((double)p.Z - old.Elevation + height);
        return GeoTerrainEdgeVertex.FromHeight(p, height, source.SampleSurfaceNormal(lat, lon));
    }
}