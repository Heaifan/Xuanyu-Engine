using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public static class GeoTerrainMeshSurface
{
    // 在当前可见三角形的 XY 投影内求 Z；不把裙边或瓦片包围盒当成地表。
    public static double? HeightAt(RenderMesh mesh, double x, double y)
    {
        ReadOnlySpan<float> v = mesh.Vertices.Span;
        double? highest = null;
        for (int i = 0; i < v.Length; i += 21)
        {
            double ax = v[i], ay = v[i + 1];
            double bx = v[i + 7], by = v[i + 8];
            double cx = v[i + 14], cy = v[i + 15];
            if (x < Math.Min(ax, Math.Min(bx, cx)) - 0.001d ||
                x > Math.Max(ax, Math.Max(bx, cx)) + 0.001d ||
                y < Math.Min(ay, Math.Min(by, cy)) - 0.001d ||
                y > Math.Max(ay, Math.Max(by, cy)) + 0.001d)
                continue;
            double det = (by - cy) * (ax - cx) + (cx - bx) * (ay - cy);
            if (Math.Abs(det) < 1e-10d) continue;
            double u = ((by - cy) * (x - cx) + (cx - bx) * (y - cy)) / det;
            double w = ((cy - ay) * (x - cx) + (ax - cx) * (y - cy)) / det;
            double t = 1d - u - w;
            if (u < -1e-5d || w < -1e-5d || t < -1e-5d) continue;
            double z = u * v[i + 2] + w * v[i + 9] + t * v[i + 16];
            highest = highest is { } old ? Math.Max(old, z) : z;
        }
        return highest;
    }
}