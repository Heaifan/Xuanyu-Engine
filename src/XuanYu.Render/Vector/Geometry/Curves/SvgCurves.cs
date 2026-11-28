using System.Numerics;

namespace XuanYu.Render;

internal static class SvgCurves
{
    public static void Quadratic(List<Vector2> points, Vector2 a, Vector2 b, Vector2 c) =>
        Cubic(points, a, a + (b-a)*(2f/3), c + (b-c)*(2f/3), c);

    public static void Cubic(List<Vector2> points, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        // 固定分段为当前兵牌资产提供确定的几何；曲线按原始 SVG 保留可编辑性。
        const int steps = 32;
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i/steps, u = 1-t;
            points.Add(u*u*u*a + 3*u*u*t*b + 3*u*t*t*c + t*t*t*d);
        }
    }
}