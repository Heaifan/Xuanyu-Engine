namespace XuanYu.Editor.Preview.Terrain;

internal sealed partial class TerrainTileGeometry
{
    private void AddCorners()
    {
        int s = _step, w = _cellsX, h = _cellsY;
        int[] xs = Border(w), ys = Border(h);

        Fan((s, s),
            ys.Where(y => y <= s).Reverse().Select(y => (X: 0, Y: y))
            .Concat(xs.Where(x => x > 0 && x <= s).Select(x => (X: x, Y: 0))));

        Fan((w - s, s),
            xs.Where(x => x >= w - s).Select(x => (X: x, Y: 0))
            .Concat(ys.Where(y => y > 0 && y <= s).Select(y => (X: w, Y: y))));

        Fan((w - s, h - s),
            ys.Where(y => y >= h - s).Select(y => (X: w, Y: y))
            .Concat(xs.Where(x => x >= w - s && x < w)
                .Reverse().Select(x => (X: x, Y: h))));

        Fan((s, h - s),
            xs.Where(x => x <= s).Reverse().Select(x => (X: x, Y: h))
            .Concat(ys.Where(y => y >= h - s && y < h)
                .Reverse().Select(y => (X: 0, Y: y))));
    }

    private void Fan((int X, int Y) center,
        IEnumerable<(int X, int Y)> boundary)
    {
        (int X, int Y)? previous = null;
        foreach (var point in boundary)
        {
            if (previous is { } first) Tri(center, first, point);
            previous = point;
        }
    }
}