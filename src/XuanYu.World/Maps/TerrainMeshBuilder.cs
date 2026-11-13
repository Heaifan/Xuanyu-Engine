
using System.Numerics;

namespace XuanYu.World.Maps;

public static class TerrainMeshBuilder
{
    public static TerrainMeshGeometry Build(TerrainHeightField field)
    {
        ArgumentNullException.ThrowIfNull(field);

        int width = field.SamplesX;
        int height = field.SamplesY;
        var positions = new Vector3[field.SampleCount];
        var indices = new int[checked((width - 1) * (height - 1) * 6)];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                positions[y * width + x] = field.GetWorldPosition(x, y);
        }

        int offset = 0;
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int a = y * width + x;
                int b = a + 1;
                int c = a + width;
                int d = c + 1;

                indices[offset++] = a;
                indices[offset++] = b;
                indices[offset++] = d;

                indices[offset++] = a;
                indices[offset++] = d;
                indices[offset++] = c;
            }
        }

        Vector3[] normals = CalculateNormals(positions, indices);
        return new TerrainMeshGeometry(positions, normals, indices);
    }

    private static Vector3[] CalculateNormals(
        Vector3[] positions, int[] indices)
    {
        var normals = new Vector3[positions.Length];

        for (int i = 0; i < indices.Length; i += 3)
        {
            int a = indices[i];
            int b = indices[i + 1];
            int c = indices[i + 2];

            Vector3 face = Vector3.Cross(
                positions[b] - positions[a],
                positions[c] - positions[a]);

            normals[a] += face;
            normals[b] += face;
            normals[c] += face;
        }

        for (int i = 0; i < normals.Length; i++)
            normals[i] = Vector3.Normalize(normals[i]);

        return normals;
    }
}
