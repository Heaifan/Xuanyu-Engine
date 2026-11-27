using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

internal static class GeoUnifiedSurface
{
    public static bool Intersects(GeoUnifiedBounds bounds, Vector3 origin, Vector3 ray, float closest)
    {
        double lo = 0, hi = closest;
        return Clip(origin.X, ray.X, bounds.Min.East, bounds.Max.East, ref lo, ref hi) &&
            Clip(origin.Y, ray.Y, bounds.Min.North, bounds.Max.North, ref lo, ref hi) &&
            Clip(origin.Z, ray.Z, bounds.Min.Up, bounds.Max.Up, ref lo, ref hi);
    }

    public static void Pick(GeoUnifiedHeightPage page, EnuFrame anchor, int mask,
        Vector4[]? clips, Vector3 origin, Vector3 ray, ref float closest,
        ref GeoUnifiedSurfaceHit hit)
    {
        var vertices = new Vector3[33 * 33];
        int step = 1 << page.Tile.Level;
        for (int y = 0; y < 33; y++)
            for (int x = 0; x < 33; x++)
            {
                var p = GeoUnifiedPosition.At(anchor, page.Tile.West + x * step,
                    page.Tile.South + y * step, page.HeightAt(x, y)!.Value);
                vertices[y * 33 + x] = new((float)p.East, (float)p.North, (float)p.Up);
            }
        int[] indices = GeoUnifiedGridIndices.Create(mask);
        for (int i = 0; i < indices.Length; i += 3)
        {
            int a = indices[i], b = indices[i + 1], c = indices[i + 2];
            if (!Triangle(origin, ray, vertices[a], vertices[b], vertices[c],
                out float t, out float u, out float v) || t >= closest) continue;
            Vector2 grid = Grid(a) * (1 - u - v) + Grid(b) * u + Grid(c) * v;
            if (clips is not null && !clips.Any(r => grid.X >= r.X && grid.X <= r.Z &&
                grid.Y >= r.Y && grid.Y <= r.W)) continue;
            closest = t;
            var heights = new Vector3(page.HeightAt(a % 33, a / 33)!.Value,
                page.HeightAt(b % 33, b / 33)!.Value, page.HeightAt(c % 33, c / 33)!.Value);
            float elevation = Vector3.Dot(heights, new Vector3(1 - u - v, u, v));
            hit = new(origin + ray * t, vertices[a], vertices[b], vertices[c], heights, elevation)
                { Tile = page.Tile };
        }
    }

    private static Vector2 Grid(int index) => new(index % 33, index / 33);

    private static bool Clip(float o, float d, double min, double max, ref double lo, ref double hi)
    {
        min = MathF.BitDecrement((float)min); max = MathF.BitIncrement((float)max);
        if (d == 0f) return o >= min && o <= max;
        double a = (min - o) / d, b = (max - o) / d;
        lo = Math.Max(lo, Math.Min(a, b)); hi = Math.Min(hi, Math.Max(a, b));
        return lo <= hi;
    }

    private static bool Triangle(Vector3 origin, Vector3 ray, Vector3 a, Vector3 b,
        Vector3 c, out float t, out float u, out float v)
    {
        t = u = v = 0;
        Vector3 ab = b - a, ac = c - a, p = Vector3.Cross(ray, ac);
        float det = Vector3.Dot(ab, p);
        if (MathF.Abs(det) < 1e-7f) return false;
        float inv = 1f / det;
        Vector3 offset = origin - a;
        u = Vector3.Dot(offset, p) * inv;
        if (u < 0f || u > 1f) return false;
        Vector3 q = Vector3.Cross(offset, ab);
        v = Vector3.Dot(ray, q) * inv;
        if (v < 0f || u + v > 1f) return false;
        t = Vector3.Dot(ac, q) * inv;
        return t > 0.001f;
    }
}