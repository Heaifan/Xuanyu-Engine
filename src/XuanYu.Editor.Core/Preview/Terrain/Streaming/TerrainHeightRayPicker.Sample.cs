using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class TerrainHeightRayPicker
{
    private static bool Clip(double origin, double direction, double size,
        ref double first, ref double last)
    {
        if (direction == 0d) return origin >= 0d && origin <= size;
        double near = -origin / direction;
        double far = (size - origin) / direction;
        if (near > far) (near, far) = (far, near);
        first = Math.Max(first, near);
        last = Math.Min(last, far);
        return first <= last;
    }

    private static double NextBoundary(double origin, double direction,
        int cell, double spacing)
    {
        if (direction == 0d) return double.PositiveInfinity;
        double boundary = (direction > 0d ? cell + 1 : cell) * spacing;
        return (boundary - origin) / direction;
    }

    private static bool Intersect(Vector3 origin, (double X, double Y, double Z) ray,
        Vector3 a, Vector3 b, Vector3 c, double enter, double exit,
        out double distance, out Vector3 point)
    {
        distance = 0d;
        point = default;
        var ab = Subtract(b, a);
        var ac = Subtract(c, a);
        var p = Cross(ray, ac);
        double determinant = Dot(ab, p);
        if (Math.Abs(determinant) < 1e-8d) return false;
        double inverse = 1d / determinant;
        var offset = Subtract(origin, a);
        double u = Dot(offset, p) * inverse;
        if (u < -1e-6d || u > 1d + 1e-6d) return false;
        var q = Cross(offset, ab);
        double v = Dot(ray, q) * inverse;
        if (v < -1e-6d || u + v > 1d + 1e-6d) return false;
        distance = Dot(ac, q) * inverse;
        if (distance < enter - 0.001d || distance > exit + 0.001d) return false;
        // 高程取命中三角形的重心插值，平坦面不受射线起点与距离相消影响。
        point = new Vector3((float)(origin.X + ray.X * distance),
            (float)(origin.Y + ray.Y * distance), (float)(a.Z + u * ab.Z + v * ac.Z));
        return true;
    }

    private static (double X, double Y, double Z) Subtract(Vector3 a, Vector3 b) =>
        ((double)a.X - b.X, (double)a.Y - b.Y, (double)a.Z - b.Z);

    private static (double X, double Y, double Z) Cross(
        (double X, double Y, double Z) a, (double X, double Y, double Z) b) =>
        (a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    private static double Dot((double X, double Y, double Z) a,
        (double X, double Y, double Z) b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
}