using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public static class GeoTerrainCursorPicker
{
    public static bool TryPick(Matrix4x4 projection, Vector2 pixel,
        uint width, uint height, IEnumerable<RenderMesh> meshes,
        out Vector3 hit)
    {
        hit = default;
        if (!ViewportScreenRay.TryCreate(projection, pixel,
            width, height, out Vector3 start, out Vector3 ray)) return false;

        float closest = float.PositiveInfinity;
        foreach (RenderMesh mesh in meshes)
        {
            ReadOnlySpan<float> vertices = mesh.Vertices.Span;
            for (int i = 0; i < vertices.Length; i += 21)
            {
                Vector3 a = Point(vertices, i);
                Vector3 b = Point(vertices, i + 7);
                Vector3 c = Point(vertices, i + 14);
                if (!Intersect(start, ray, a, b, c, out float distance) ||
                    distance >= closest) continue;
                closest = distance;
                hit = start + ray * distance;
            }
        }
        return float.IsFinite(closest);
    }

    private static Vector3 Point(ReadOnlySpan<float> data, int index) =>
        new(data[index], data[index + 1], data[index + 2]);

    private static bool Intersect(Vector3 origin, Vector3 ray,
        Vector3 a, Vector3 b, Vector3 c, out float distance)
    {
        distance = 0f;
        Vector3 ab = b - a, ac = c - a;
        Vector3 p = Vector3.Cross(ray, ac);
        float determinant = Vector3.Dot(ab, p);
        if (MathF.Abs(determinant) < 1e-7f) return false;
        float inverse = 1f / determinant;
        Vector3 offset = origin - a;
        float u = Vector3.Dot(offset, p) * inverse;
        if (u < 0f || u > 1f) return false;
        Vector3 q = Vector3.Cross(offset, ab);
        float v = Vector3.Dot(ray, q) * inverse;
        if (v < 0f || u + v > 1f) return false;
        distance = Vector3.Dot(ac, q) * inverse;
        return distance > 0.001f;
    }
}