using System.Numerics;
using System.Runtime.CompilerServices;
using XuanYu.Editor.Camera;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public static class GeoTerrainCursorPicker
{
    private static readonly bool UseBounds = Environment.GetEnvironmentVariable("XYE_G06_PICK_BOUNDS") != "0";
    private static readonly ConditionalWeakTable<RenderMesh, MeshBounds> Bounds = new();

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
            if (UseBounds && !Bounds.GetValue(mesh, static m => new MeshBounds(m)).Intersects(start, ray))
                continue;
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
    private sealed class MeshBounds
    {
        private readonly Vector3 _min, _max;

        public MeshBounds(RenderMesh mesh)
        {
            var vertices = mesh.Vertices.Span;
            Vector3 min = Point(vertices, 0), max = min;
            for (int i = 7; i < vertices.Length; i += 7)
            {
                Vector3 p = Point(vertices, i);
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
            _min = new(MathF.BitDecrement(min.X), MathF.BitDecrement(min.Y), MathF.BitDecrement(min.Z));
            _max = new(MathF.BitIncrement(max.X), MathF.BitIncrement(max.Y), MathF.BitIncrement(max.Z));
        }

        public bool Intersects(Vector3 origin, Vector3 ray)
        {
            double lo = 0d, hi = double.PositiveInfinity;
            return Clip(origin.X, ray.X, _min.X, _max.X, ref lo, ref hi) &&
                Clip(origin.Y, ray.Y, _min.Y, _max.Y, ref lo, ref hi) &&
                Clip(origin.Z, ray.Z, _min.Z, _max.Z, ref lo, ref hi);
        }

        private static bool Clip(float o, float d, float min, float max, ref double lo, ref double hi)
        {
            if (d == 0f) return o >= min && o <= max;
            double a = ((double)min - o) / d, b = ((double)max - o) / d;
            lo = Math.Max(lo, Math.Min(a, b));
            hi = Math.Min(hi, Math.Max(a, b));
            return lo <= hi;
        }
    }
}