
using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class TerrainRenderMeshAdapter
{
    public static RenderMesh Create(TerrainMeshGeometry geometry)
    {
        float[] values = new float[checked(geometry.Indices.Length * 7)];
        Vector3 sun = Vector3.Normalize(
            new Vector3(-0.5f, -0.3f, 1f));

        Vector3 grass = new(0.18f, 0.54f, 0.26f);
        Vector3 rock = new(0.49f, 0.47f, 0.39f);

        for (int i = 0; i < geometry.Indices.Length; i++)
        {
            int vertex = geometry.Indices[i];
            Vector3 point = geometry.Positions[vertex];
            Vector3 normal = geometry.Normals[vertex];

            float slope = Math.Clamp(
                (1f - normal.Z) * 3f, 0f, 1f);
            float light = 0.58f + 0.42f * MathF.Max(
                0f, Vector3.Dot(normal, sun));

            Vector3 color = Vector3.Lerp(
                grass, rock, slope) * light;

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
}
