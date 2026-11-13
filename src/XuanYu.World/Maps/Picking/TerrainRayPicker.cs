
using System.Numerics;

namespace XuanYu.World.Maps;

public static class TerrainRayPicker
{
    public static bool TryPick(TerrainMeshGeometry mesh, Vector3 origin,
        Vector3 direction, out TerrainRayHit hit)
    {
        float lengthSquared = direction.LengthSquared();
        if (!(lengthSquared > 0f) || !float.IsFinite(lengthSquared))
            throw new ArgumentException("射线方向必须是有限的非零向量。", nameof(direction));

        Vector3 ray = direction / MathF.Sqrt(lengthSquared);
        Vector3[] vertices = mesh.Positions;
        int[] indices = mesh.Indices;
        float closest = float.PositiveInfinity;
        int selectedTriangle = -1;

        for (int i = 0; i < indices.Length; i += 3)
        {
            Vector3 a = vertices[indices[i]];
            Vector3 b = vertices[indices[i + 1]];
            Vector3 c = vertices[indices[i + 2]];
            if (!Intersect(origin, ray, a, b, c, out float distance))
                continue;
            if (distance >= closest)
                continue;
            closest = distance;
            selectedTriangle = i / 3;
        }

        if (selectedTriangle < 0)
        {
            hit = default;
            return false;
        }

        hit = new TerrainRayHit(origin + ray * closest,
            selectedTriangle, closest);
        return true;
    }

    private static bool Intersect(Vector3 origin, Vector3 ray,
        Vector3 a, Vector3 b, Vector3 c, out float distance)
    {
        distance = 0f;
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 p = Vector3.Cross(ray, ac);
        float determinant = Vector3.Dot(ab, p);

        // 双面拾取，避免从地形背面观察时错误地漏选。
        if (MathF.Abs(determinant) < 1e-8f)
            return false;

        float inverse = 1f / determinant;
        Vector3 offset = origin - a;
        float u = Vector3.Dot(offset, p) * inverse;
        if (u < -1e-6f || u > 1f + 1e-6f)
            return false;

        Vector3 q = Vector3.Cross(offset, ab);
        float v = Vector3.Dot(ray, q) * inverse;
        if (v < -1e-6f || u + v > 1f + 1e-6f)
            return false;

        distance = Vector3.Dot(ac, q) * inverse;
        return distance >= 0f;
    }
}
