using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class TerrainHeightRayPicker
{
    public static bool TryPick(TerrainHeightField field, Vector3 origin,
        Vector3 direction, out TerrainRayHit hit)
    {
        hit = default;
        var input = ((double)direction.X, (double)direction.Y, (double)direction.Z);
        double length = Math.Sqrt(Dot(input, input));
        if (!double.IsFinite(length) || length <= 0d)
            throw new ArgumentException("射线方向无效。", nameof(direction));
        var ray = (X: input.Item1 / length, Y: input.Item2 / length, Z: input.Item3 / length);
        double enter = 0d, exit = double.PositiveInfinity;
        if (!Clip(origin.X, ray.X, field.WidthMeters, ref enter, ref exit) ||
            !Clip(origin.Y, ray.Y, field.LengthMeters, ref enter, ref exit))
            return false;

        double x = origin.X + ray.X * (enter + 0.0001d);
        double y = origin.Y + ray.Y * (enter + 0.0001d);
        int cx = Math.Clamp((int)(x / field.SpacingXMeters), 0, field.SamplesX - 2);
        int cy = Math.Clamp((int)(y / field.SpacingYMeters), 0, field.SamplesY - 2);
        int sx = Math.Sign(ray.X), sy = Math.Sign(ray.Y);
        double nextX = NextBoundary(origin.X, ray.X, cx, field.SpacingXMeters);
        double nextY = NextBoundary(origin.Y, ray.Y, cy, field.SpacingYMeters);
        double deltaX = sx == 0 ? double.PositiveInfinity :
            field.SpacingXMeters / Math.Abs(ray.X);
        double deltaY = sy == 0 ? double.PositiveInfinity :
            field.SpacingYMeters / Math.Abs(ray.Y);

        for (int n = 0; n < field.SamplesX + field.SamplesY; n++)
        {
            double boundary = Math.Min(exit, Math.Min(nextX, nextY));
            if (TryCell(field, cx, cy, origin, ray, enter, boundary, out hit))
                return true;
            if (boundary >= exit) break;
            bool moveX = nextX <= nextY;
            bool moveY = nextY <= nextX;
            if (moveX) { cx += sx; nextX += deltaX; }
            if (moveY) { cy += sy; nextY += deltaY; }
            if (cx < 0 || cy < 0 ||
                cx >= field.SamplesX - 1 || cy >= field.SamplesY - 1)
                break;
            enter = boundary;
        }
        return false;
    }

    private static bool TryCell(TerrainHeightField field, int x, int y,
        Vector3 origin, (double X, double Y, double Z) ray, double enter, double exit,
        out TerrainRayHit hit)
    {
        hit = default;
        Vector3 a = field.GetWorldPosition(x, y);
        Vector3 b = field.GetWorldPosition(x + 1, y);
        Vector3 c = field.GetWorldPosition(x, y + 1);
        Vector3 d = field.GetWorldPosition(x + 1, y + 1);
        int first = 2 * (y * (field.SamplesX - 1) + x);
        if (Intersect(origin, ray, a, b, d, enter, exit, out double t, out var point))
        {
            hit = new TerrainRayHit(point, first, (float)t);
            return true;
        }
        if (Intersect(origin, ray, a, d, c, enter, exit, out t, out point))
        {
            hit = new TerrainRayHit(point, first + 1, (float)t);
            return true;
        }
        return false;
    }
}