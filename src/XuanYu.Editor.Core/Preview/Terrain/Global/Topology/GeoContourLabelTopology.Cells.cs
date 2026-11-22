namespace XuanYu.Editor.Preview.Terrain;

internal sealed partial class GeoContourLabelTopology
{
    public void ConnectCell(GeoContourLabelSample[,] grid, int row, int col, int interval)
    {
        double? sw = grid[row, col].Height;
        double? se = grid[row, col + 1].Height;
        double? ne = grid[row + 1, col + 1].Height;
        double? nw = grid[row + 1, col].Height;
        if (sw is null || se is null || ne is null || nw is null) return;
        double min = Math.Min(Math.Min(sw.Value, se.Value), Math.Min(ne.Value, nw.Value));
        double max = Math.Max(Math.Max(sw.Value, se.Value), Math.Max(ne.Value, nw.Value));
        int first = (int)Math.Floor(min / interval) + 1;
        int last = (int)Math.Ceiling(max / interval) - 1;
        double center = (sw.Value + se.Value + ne.Value + nw.Value) * 0.25d;
        Span<int> hits = stackalloc int[4];
        for (int level = first; level <= last; level++)
        {
            int elevation = level * interval;
            // 顺时针：北、东、南、西；只连接同一海拔的穿越边。
            Span<GeoContourGridEdge> edges =
            [
                new(row + 1, col, false, elevation),
                new(row, col + 1, true, elevation),
                new(row, col, false, elevation),
                new(row, col, true, elevation)
            ];
            int count = 0;
            for (int k = 0; k < edges.Length; k++)
                if (_parents.ContainsKey(edges[k])) hits[count++] = k;
            if (count == 2)
                Join(edges[hits[0]], edges[hits[1]]);
            else if (count == 4)
            {
                // 鞍点的两种连接方式用单元格中心高度区分。
                if ((nw.Value > elevation) == (center > elevation))
                {
                    Join(edges[0], edges[1]);
                    Join(edges[2], edges[3]);
                }
                else
                {
                    Join(edges[0], edges[3]);
                    Join(edges[1], edges[2]);
                }
            }
        }
    }
}