using System.Numerics;

namespace XuanYu.Render;

internal static class SvgArc
{
    public static void Append(List<Vector2> points, Vector2 start, Vector2 end,
        double rx, double ry, double degrees, bool large, bool sweep)
    {
        if (start == end) return;
        rx = Math.Abs(rx); ry = Math.Abs(ry);
        if (rx == 0 || ry == 0) { points.Add(end); return; }
        double phi = degrees*Math.PI/180, c = Math.Cos(phi), s = Math.Sin(phi);
        double dx = ((double)start.X-end.X)/2, dy = ((double)start.Y-end.Y)/2;
        double x = c*dx+s*dy, y = -s*dx+c*dy;
        double scale = Math.Sqrt(x*x/(rx*rx)+y*y/(ry*ry));
        if (scale > 1) { rx *= scale; ry *= scale; }
        double ux = x/rx, uy = y/ry, norm = ux*ux+uy*uy;
        double factor = (large == sweep ? -1 : 1)*Math.Sqrt(Math.Max(0, (1-norm)/norm));
        double cx = factor*rx*uy, cy = -factor*ry*ux;
        double centerX = c*cx-s*cy+((double)start.X+end.X)/2;
        double centerY = s*cx+c*cy+((double)start.Y+end.Y)/2;
        double from = Math.Atan2((y-cy)/ry, (x-cx)/rx);
        double to = Math.Atan2((-y-cy)/ry, (-x-cx)/rx);
        double delta = to-from;
        if (sweep && delta < 0) delta += Math.Tau;
        if (!sweep && delta > 0) delta -= Math.Tau;
        int steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(delta)*64/Math.Tau));
        for (int i = 1; i < steps; i++)
        {
            double t = from+delta*i/steps, px = rx*Math.Cos(t), py = ry*Math.Sin(t);
            points.Add(new((float)(centerX+c*px-s*py), (float)(centerY+s*px+c*py)));
        }
        points.Add(end);
    }
}