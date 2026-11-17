using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainEdgeLayout
{
    public static double[] Fit(GeoTileBounds fine, GeoTileBounds coarse,
        int cells, bool horizontal)
    {
        double start = horizontal ? fine.WestLongitudeDegrees : fine.NorthLatitudeDegrees;
        double end = horizontal ? fine.EastLongitudeDegrees : fine.SouthLatitudeDegrees;
        double coarseStart = horizontal ? coarse.WestLongitudeDegrees :
            coarse.NorthLatitudeDegrees;
        double coarseEnd = horizontal ? coarse.EastLongitudeDegrees :
            coarse.SouthLatitudeDegrees;
        var knots = new List<double> { 0d, 1d };
        for (int i = 1; i < cells; i++)
        {
            double coordinate = coarseStart + (coarseEnd - coarseStart) * i / cells;
            double t = (coordinate - start) / (end - start);
            if (t > 1e-10d && t < 1d - 1e-10d)
                knots.Add(t);
        }
        knots.Sort();
        for (int i = knots.Count - 1; i > 0; i--)
            if (knots[i] - knots[i - 1] < 1e-10d)
                knots.RemoveAt(i);
        if (knots.Count > cells + 1)
            throw new InvalidOperationException("粗瓦片边界节点超过细瓦片容量。");
        while (knots.Count < cells + 1)
        {
            int index = 0;
            double widest = -1d;
            for (int i = 0; i < knots.Count - 1; i++)
                if (knots[i + 1] - knots[i] > widest)
                {
                    index = i;
                    widest = knots[i + 1] - knots[i];
                }
            knots.Insert(index + 1, (knots[index] + knots[index + 1]) * 0.5d);
        }
        return knots.ToArray();
    }
}