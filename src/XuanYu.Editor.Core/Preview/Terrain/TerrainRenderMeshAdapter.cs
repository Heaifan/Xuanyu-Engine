using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class TerrainRenderMeshAdapter
{
    public static RenderMesh Create(TerrainMeshGeometry geometry,
        TerrainDisplayMode mode = TerrainDisplayMode.Standard,
        TerrainWaterMask? water = null, int startX = 0, int startY = 0,
        float spacingX = 1f, float spacingY = 1f,
        (float Min, float Max)? heightRange = null,
        TerrainShadingField? shading = null)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        if (mode is not (TerrainDisplayMode.Standard or TerrainDisplayMode.Elevation))
            throw new ArgumentOutOfRangeException(nameof(mode));
        Vector3[] colors = new Vector3[geometry.Positions.Length];
        for (int vertex = 0; vertex < colors.Length; vertex++)
        {
            Vector3 point = geometry.Positions[vertex];
            Vector3 normal = geometry.Normals[vertex];
            int x = startX + (int)MathF.Round(point.X / spacingX);
            int y = startY + (int)MathF.Round(point.Y / spacingY);
            bool sea = water is not null && water.IsWater(x, y);
            bool coast = !sea && water is not null && water.IsCoastLand(x, y);
            if (mode == TerrainDisplayMode.Standard &&
                shading is not null && !sea && !coast)
            {
                colors[vertex] = shading.Sample(x, y);
                continue;
            }
            if (!sea && !coast)
            {
                Vector3 lightingNormal = mode == TerrainDisplayMode.Standard
                    ? normal : Vector3.UnitZ;
                colors[vertex] = GeoTerrainEdgeVertex.FromHeight(
                    point, point.Z, lightingNormal).ShadedColor;
                continue;
            }
            Vector3 color = sea ? new Vector3(0.05f, 0.29f, 0.52f)
                : new Vector3(0.68f, 0.61f, 0.43f);
            float light = 0.58f + 0.42f * MathF.Max(0f,
                Vector3.Dot(normal, Sun));
            colors[vertex] = color * light;
        }
        float[] values = new float[checked(geometry.Indices.Length * 7)];
        for (int i = 0; i < geometry.Indices.Length; i++)
        {
            int vertex = geometry.Indices[i];
            Vector3 point = geometry.Positions[vertex];
            Vector3 color = colors[vertex];
            int offset = i * 7;
            values[offset] = point.X;
            values[offset + 1] = point.Y;
            values[offset + 2] = point.Z;
            values[offset + 3] = color.X;
            values[offset + 4] = color.Y;
            values[offset + 5] = color.Z;
            values[offset + 6] = 1f;
        }
        return new RenderMesh("terrain/e4-preview", values);
    }

    public static RenderMesh CreateLocal(string assetId, TerrainMeshGeometry geometry)
    {
        var vertices = new GeoTerrainEdgeVertex[geometry.Positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 p = geometry.Positions[i];
            vertices[i] = GeoTerrainEdgeVertex.FromHeight(p, p.Z, geometry.Normals[i]);
        }
        var packed = new List<float>(checked(geometry.Indices.Length * 10));
        foreach (int index in geometry.Indices)
            vertices[index].AppendTo(packed);
        return RenderMesh.FromLitTerrain(assetId,
            System.Runtime.InteropServices.CollectionsMarshal.AsSpan(packed));
    }

    private static readonly Vector3 Sun = Vector3.Normalize(
        new Vector3(-0.5f, -0.3f, 1f));
}