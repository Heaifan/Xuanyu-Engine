namespace XuanYu.Editor.Preview.Terrain;

internal sealed partial class TerrainTileGeometry
{
    private void AddEdges()
    {
        AddSide(vertical: false, far: false);
        AddSide(vertical: false, far: true);
        AddSide(vertical: true, far: false);
        AddSide(vertical: true, far: true);
        AddCorners();
    }

    // All LODs share this boundary lattice, including the step=1 corner cuts.
    private static int[] Border(int length)
    {
        var points = new List<int>();
        for (int i = 0; i <= length; i++)
            if (i == 1 || i == length - 1 || i % 5 == 0 || i == length)
                points.Add(i);
        return points.ToArray();
    }

    private void AddSide(bool vertical, bool far)
    {
        int length = vertical ? _cellsY : _cellsX;
        int[] outer = Border(length).Where(x =>
            x >= _step && x <= length - _step).ToArray();
        int[] inner = Enumerable.Range(0,
            (length - 2 * _step) / _step + 1)
            .Select(i => (i + 1) * _step).ToArray();
        int a = 0, b = 0;
        bool reverse = vertical != far;
        while (a + 1 < outer.Length || b + 1 < inner.Length)
        {
            if (a + 1 < outer.Length &&
                (b + 1 == inner.Length || outer[a + 1] <= inner[b + 1]))
            {
                SideTri(Point(outer[a], 0, vertical, far),
                    Point(outer[a + 1], 0, vertical, far),
                    Point(inner[b], _step, vertical, far), reverse);
                a++;
            }
            else
            {
                SideTri(Point(outer[a], 0, vertical, far),
                    Point(inner[b + 1], _step, vertical, far),
                    Point(inner[b], _step, vertical, far), reverse);
                b++;
            }
        }
    }

    private (int X, int Y) Point(int along, int inset, bool vertical, bool far) =>
        vertical ? (far ? _cellsX - inset : inset, along) :
        (along, far ? _cellsY - inset : inset);

    private void SideTri((int X, int Y) a, (int X, int Y) b,
        (int X, int Y) c, bool reverse)
    {
        if (reverse) Tri(a, c, b);
        else Tri(a, b, c);
    }
}