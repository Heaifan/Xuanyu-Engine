using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public static partial class GeoUnifiedError
{
    private static bool MeasureEdge(Vertex a, Vertex b, GeoUnifiedTile tile,
        GeoUnifiedSamples source, EnuFrame anchor, ref double maxSquared)
    {
        int dx = b.X - a.X, dy = b.Y - a.Y;
        if (dx == 0 || dy == 0 || dx == dy) return true;
        for (int x = Math.Min(a.X, b.X) + 1; x < Math.Max(a.X, b.X); x++)
            if (!CheckCrossing(a, b, (x - a.X) / (double)dx, tile, source, anchor, ref maxSquared)) return false;
        for (int y = Math.Min(a.Y, b.Y) + 1; y < Math.Max(a.Y, b.Y); y++)
            if (!CheckCrossing(a, b, (y - a.Y) / (double)dy, tile, source, anchor, ref maxSquared)) return false;
        int da = a.X - a.Y, db = b.X - b.Y;
        for (int d = Math.Min(da, db) + 1; d < Math.Max(da, db); d++)
            if (!CheckCrossing(a, b, (d - da) / (double)(db - da), tile, source, anchor, ref maxSquared)) return false;
        return true;
    }

    private static bool CheckCrossing(Vertex a, Vertex b, double t, GeoUnifiedTile tile,
        GeoUnifiedSamples source, EnuFrame anchor, ref double maxSquared)
    {
        double x = a.X + (b.X - a.X) * t, y = a.Y + (b.Y - a.Y) * t;
        int ix = Math.Min((int)Math.Floor(x), tile.Span - 1);
        int iy = Math.Min((int)Math.Floor(y), tile.Span - 1);
        double u = x - ix, v = y - iy;
        if (!Read(ix, iy, out var p00) || !Read(ix + 1, iy + 1, out var p11)) return false;
        EnuPosition actual;
        if (u >= v)
        {
            if (!Read(ix + 1, iy, out var p10)) return false;
            actual = Blend(p00, p10, p11, 1 - u, u - v, v);
        }
        else
        {
            if (!Read(ix, iy + 1, out var p01)) return false;
            actual = Blend(p00, p11, p01, 1 - v, u, v - u);
        }
        var simplified = Blend(a.Position, b.Position, b.Position, 1 - t, t, 0);
        double dx = actual.East - simplified.East, dy = actual.North - simplified.North;
        double dz = actual.Up - simplified.Up;
        maxSquared = Math.Max(maxSquared, dx * dx + dy * dy + dz * dz);
        return true;

        bool Read(int col, int row, out EnuPosition result)
        {
            var sample = source.Sample(tile.West + col, tile.South + row);
            if (sample.Status == GeoSampleStatus.Valid && sample.Height is { } height)
            { result = Position(source, anchor, tile.West + col, tile.South + row, height); return true; }
            result = default;
            return false;
        }
    }

    private static EnuPosition Blend(EnuPosition a, EnuPosition b, EnuPosition c,
        double wa, double wb, double wc) => new(
            a.East * wa + b.East * wb + c.East * wc,
            a.North * wa + b.North * wb + c.North * wc,
            a.Up * wa + b.Up * wb + c.Up * wc);
}