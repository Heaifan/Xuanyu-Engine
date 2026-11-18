namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainNativeMeshBuilder
{
    private static double[] Axis(double min, double max, IEnumerable<double> anchors)
    {
        var points = new List<double> { min, max };
        // 所有 HGT 共享整数度起点的 1/3600° 栅格，不再截断于单张 HGT。
        double origin = Math.Floor(min);
        int first = (int)Math.Ceiling((min - origin) * 3600d);
        int last = (int)Math.Floor((max - origin) * 3600d);
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
}