
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
        (float Min, float Max)? heightRange = null)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        if (mode is not (TerrainDisplayMode.Standard or TerrainDisplayMode.Elevation))
            throw new ArgumentOutOfRangeException(nameof(mode));
        (float min, float max) = heightRange ?? HeightRange(geometry.Positions);
        float range = max - min;
        Vector3 sun = Vector3.Normalize(new Vector3(-0.5f, -0.3f, 1f));
        Vector3[] colors = new Vector3[geometry.Positions.Length];
        for (int vertex = 0; vertex < colors.Length; vertex++)
        {
            Vector3 point = geometry.Positions[vertex];
            Vector3 normal = geometry.Normals[vertex];
            float height = range > 0.0001f
                ? (point.Z - min) / range : 0.25f;
            Vector3 color = mode == TerrainDisplayMode.Standard
                ? TerrainColorPalette.Standard(normal, height)
                : TerrainColorPalette.Elevation(height);
            if (water is not null)
            {
                int x = startX + (int)MathF.Round(point.X / spacingX);
                int y = startY + (int)MathF.Round(point.Y / spacingY);
                if (water.IsWater(x, y))
                    color = new Vector3(0.05f, 0.29f, 0.52f);
                else if (water.IsCoastLand(x, y))
                    color = new Vector3(0.68f, 0.61f, 0.43f);
            }
            float light = 0.58f + 0.42f * MathF.Max(
                0f, Vector3.Dot(normal, sun));
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

    private static (float Min, float Max) HeightRange(Vector3[] points)
    {
        float min = float.PositiveInfinity, max = float.NegativeInfinity;
        foreach (Vector3 point in points)
        {
            min = MathF.Min(min, point.Z);
            max = MathF.Max(max, point.Z);
        }
        return (min, max);
    }
}
