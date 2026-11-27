using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public readonly record struct GeoUnifiedBounds(EnuPosition Min, EnuPosition Max)
{
    public double DistanceTo(EnuPosition point)
    {
        double x = Math.Max(0, Math.Max(Min.East - point.East, point.East - Max.East));
        double y = Math.Max(0, Math.Max(Min.North - point.North, point.North - Max.North));
        double z = Math.Max(0, Math.Max(Min.Up - point.Up, point.Up - Max.Up));
        return Math.Sqrt(x * x + y * y + z * z);
    }

    public static GeoUnifiedBounds? Measure(GeoUnifiedTile tile, GeoUnifiedSamples source, EnuFrame anchor)
    {
        using var pin = source.PinResident();
        double minX = double.PositiveInfinity, minY = minX, minZ = minX;
        double maxX = double.NegativeInfinity, maxY = maxX, maxZ = maxX;
        for (int row = 0; row <= tile.Span; row++)
            for (int col = 0; col <= tile.Span; col++)
            {
                int x = tile.West + col, y = tile.South + row;
                var sample = source.Sample(x, y);
                if (sample.Status != GeoSampleStatus.Valid || sample.Height is not { } height) return null;
                var p = GeoUnifiedPosition.At(anchor, x, y, height);
                minX = Math.Min(minX, p.East); maxX = Math.Max(maxX, p.East);
                minY = Math.Min(minY, p.North); maxY = Math.Max(maxY, p.North);
                minZ = Math.Min(minZ, p.Up); maxZ = Math.Max(maxZ, p.Up);
            }
        return new(new(minX, minY, minZ), new(maxX, maxY, maxZ));
    }
}