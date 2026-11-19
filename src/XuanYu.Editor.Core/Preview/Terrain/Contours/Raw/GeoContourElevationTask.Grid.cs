namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoContourElevationTask
{
    private static double[] Axis(double min, double max, int step)
    {
        if (max <= min) return [];
        var axis = new List<double> { min };
        int first = (int)Math.Floor(min * 3600d / step) + 1;
        int last = (int)Math.Ceiling(max * 3600d / step) - 1;
        for (int i = first; i <= last; i++)
        {
            double value = i * step / 3600d;
            if (value > min + 1e-11d && value < max - 1e-11d)
                axis.Add(value);
        }
        axis.Add(max);
        return axis.ToArray();
    }

    private double?[] SampleRow(int y)
    {
        var values = new double?[_xs.Length];
        for (int x = 0; x < values.Length; x++)
            values[x] = _source.Sample(_ys[y], _xs[x]);
        return values;
    }
}