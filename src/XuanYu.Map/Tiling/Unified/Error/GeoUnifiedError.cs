using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public static partial class GeoUnifiedError
{
    private readonly record struct Vertex(int X, int Y, EnuPosition Position);

    public static double? Measure(GeoUnifiedHeightPage page, GeoUnifiedSamples source,
        EnuFrame anchor, int edgeMask)
    {
        using var pin = source.PinResident();
        if (page.Identity != source.Identity)
            throw new InvalidOperationException("高程页和参考高程的数据版本不一致。");
        GeoUnifiedTile tile = page.Tile;
        int step = 1 << tile.Level, stride = source.ReferenceStride(tile);
        var vertices = new Vertex[33 * 33];
        for (int y = 0; y < 33; y++)
            for (int x = 0; x < 33; x++)
            {
                if (page.StatusAt(x, y) != GeoSampleStatus.Valid || page.HeightAt(x, y) is null)
                    return null;
                int px = x * step, py = y * step;
                vertices[y * 33 + x] = new(px, py, page.PositionAt(anchor, x, y));
            }
        var indices = GeoUnifiedGridIndices.Create(edgeMask);
        double maxSquared = 0;
        for (int i = 0; i < indices.Length; i += 3)
        {
            Vertex a = vertices[indices[i]], b = vertices[indices[i + 1]], c = vertices[indices[i + 2]];
            if (!MeasureEdge(a, b, tile, source, anchor, ref maxSquared) ||
                !MeasureEdge(b, c, tile, source, anchor, ref maxSquared) ||
                !MeasureEdge(c, a, tile, source, anchor, ref maxSquared)) return null;
            long area = Cross(b.X - a.X, b.Y - a.Y, c.X - a.X, c.Y - a.Y);
            for (int y = Math.Min(a.Y, Math.Min(b.Y, c.Y)); y <= Math.Max(a.Y, Math.Max(b.Y, c.Y)); y += stride)
                for (int x = Math.Min(a.X, Math.Min(b.X, c.X)); x <= Math.Max(a.X, Math.Max(b.X, c.X)); x += stride)
                {
                    long wa = Cross(b.X - x, b.Y - y, c.X - x, c.Y - y);
                    long wb = Cross(c.X - x, c.Y - y, a.X - x, a.Y - y);
                    long wc = area - wa - wb;
                    if (wa < 0 || wb < 0 || wc < 0) continue;
                    var sample = source.Sample(tile.West + x, tile.South + y);
                    if (sample.Status != GeoSampleStatus.Valid || sample.Height is not { } height) return null;
                    EnuPosition actual = Position(source, anchor, tile.West + x, tile.South + y, height);
                    double ax = (a.Position.East * wa + b.Position.East * wb + c.Position.East * wc) / area;
                    double ay = (a.Position.North * wa + b.Position.North * wb + c.Position.North * wc) / area;
                    double az = (a.Position.Up * wa + b.Position.Up * wb + c.Position.Up * wc) / area;
                    double dx = actual.East - ax, dy = actual.North - ay, dz = actual.Up - az;
                    maxSquared = Math.Max(maxSquared, dx * dx + dy * dy + dz * dz);
                }
        }
        return Math.Sqrt(maxSquared);
    }

    private static long Cross(long ax, long ay, long bx, long by) => ax * by - ay * bx;

    private static EnuPosition Position(GeoUnifiedSamples source, EnuFrame anchor, int x, int y, short height) =>
        GeoUnifiedPosition.At(anchor, x, y, height, source.GeoidCentimetersAt(x, y));
}