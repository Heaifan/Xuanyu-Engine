using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

internal static partial class GeoTerrainGridMesh
{
    private static void AddDrapedLine(List<float> vertices,
        Func<double, double, Vector3?> point,
        double u0, double v0, double u1, double v1,
        float width, Vector3 color, int detail)
    {
        int count = (int)Math.Clamp(detail *
            (Math.Abs(u1 - u0) + Math.Abs(v1 - v0)), 1d, 128d);
        Vector3? previous = point(u0, v0);
        for (int i = 1; i <= count; i++)
        {
            double t = i / (double)count;
            Vector3? next = point(u0 + (u1 - u0) * t,
                v0 + (v1 - v0) * t);
            if (previous is { } a && next is { } b)
            {
                Vector3 side = new(b.Y - a.Y, a.X - b.X, 0f);
                if (side.LengthSquared() > 0.0001f)
                {
                    side = Vector3.Normalize(side) * width;
                    Add(a + side); Add(a - side); Add(b + side);
                    Add(a - side); Add(b - side); Add(b + side);
                }
            }
            previous = next;
        }

        void Add(Vector3 p)
        {
            vertices.Add(p.X); vertices.Add(p.Y); vertices.Add(p.Z);
            vertices.Add(color.X); vertices.Add(color.Y); vertices.Add(color.Z);
            vertices.Add(1f);
        }
    }
}