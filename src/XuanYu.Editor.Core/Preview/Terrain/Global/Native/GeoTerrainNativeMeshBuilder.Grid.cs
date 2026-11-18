using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainNativeMeshBuilder
{
    private static double[] Axis(double min, double max, double origin,
        IEnumerable<double> anchors)
    {
        var points = new List<double> { min, max };
        int first = Math.Max(0, (int)Math.Ceiling((min - origin) * 3600d));
        int last = Math.Min(3600, (int)Math.Floor((max - origin) * 3600d));
        for (int i = first; i <= last; i++)
        {
            double value = origin + i / 3600d;
            if (value > min && value < max) points.Add(value);
        }
        points.AddRange(anchors.Where(v => v > min && v < max));
        points.Sort();
        var unique = new List<double>(points.Count);
        foreach (double value in points)
            if (unique.Count == 0 || value - unique[^1] > 1e-11d)
                unique.Add(value);
        return unique.ToArray();
    }

    private static double? ReadHeight(NasademTile tile,
        NasademElevationSource source, double longitude, double latitude,
        Dictionary<(int, int), NasademSampleBlock> blocks)
    {
        double x = (longitude - tile.WestLongitude) * 3600d;
        double y = (latitude - tile.SouthLatitude) * 3600d;
        int col = (int)Math.Round(x), row = (int)Math.Round(y);
        if (Math.Abs(x - col) > 1e-7d || Math.Abs(y - row) > 1e-7d)
            return source.Sample(latitude, longitude);
        int bx = Math.Min(col / 300, 11), by = Math.Min(row / 300, 11);
        if (!blocks.TryGetValue((bx, by), out NasademSampleBlock? block))
        {
            block = new NasademSampleBlock(tile, bx, by);
            blocks.Add((bx, by), block);
        }
        return block.HeightAt(col - block.StartColumn,
            row - block.StartRowFromSouth);
    }
}