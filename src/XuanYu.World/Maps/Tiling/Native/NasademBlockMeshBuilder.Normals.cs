using System.Numerics;

namespace XuanYu.World.Maps;

public static partial class NasademBlockMeshBuilder
{
    private static Vector3[] CalculateNormals(Vector3[] positions,
        List<int> indices)
    {
        var normals = new Vector3[positions.Length];
        for (int i = 0; i < indices.Count; i += 3)
        {
            int a = indices[i];
            int b = indices[i + 1];
            int c = indices[i + 2];
            Vector3 face = Vector3.Cross(
                positions[b] - positions[a], positions[c] - positions[a]);
            normals[a] += face;
            normals[b] += face;
            normals[c] += face;
        }

        for (int i = 0; i < normals.Length; i++)
            normals[i] = normals[i].LengthSquared() > 1e-12f
                ? Vector3.Normalize(normals[i]) : Vector3.UnitZ;
        return normals;
    }
}