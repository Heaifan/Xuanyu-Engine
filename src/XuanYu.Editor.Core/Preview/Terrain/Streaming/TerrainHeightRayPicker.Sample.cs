using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class TerrainHeightRayPicker
{
    private static bool Clip(float origin, float direction, float size,
        ref float first, ref float last)
    {
        if (direction == 0f)
            return origin >= 0f && origin <= size;
        float near = -origin / direction;
        float far = (size - origin) / direction;
        if (near > far) (near, far) = (far, near);
        first = MathF.Max(first, near);
        last = MathF.Min(last, far);
        return first <= last;
    }

    private static float NextBoundary(float origin, float direction,
        int cell, float spacing)
    {
        if (direction == 0f) return float.PositiveInfinity;
        float boundary = (direction > 0f ? cell + 1 : cell) * spacing;
        return (boundary - origin) / direction;
    }

    private static bool Intersect(Vector3 origin, Vector3 ray,
        Vector3 a, Vector3 b, Vector3 c,
        float enter, float exit, out float distance)
    {
        distance = 0f;
        Vector3 ab = b - a, ac = c - a;
        Vector3 p = Vector3.Cross(ray, ac);
        float determinant = Vector3.Dot(ab, p);
        if (MathF.Abs(determinant) < 1e-8f) return false;
        float inverse = 1f / determinant;
        Vector3 offset = origin - a;
        float u = Vector3.Dot(offset, p) * inverse;
        if (u < -1e-6f || u > 1f + 1e-6f) return false;
        Vector3 q = Vector3.Cross(offset, ab);
        float v = Vector3.Dot(ray, q) * inverse;
        if (v < -1e-6f || u + v > 1f + 1e-6f) return false;
        distance = Vector3.Dot(ac, q) * inverse;
        return distance >= enter - 0.001f && distance <= exit + 0.001f;
    }
}