namespace XuanYu.Editor.TerrainEditing;

internal sealed class TerrainHydraulicFlow
{
    private readonly int _width, _length;
    private readonly double _sx, _sy, _diagonal, _cellArea;
    private readonly int[] _incoming;
    public int[] Receiver { get; }
    public int[] Order { get; }
    public double[] Slope { get; }
    public double[] Area { get; }

    public TerrainHydraulicFlow(int width, int length, float sx, float sy)
    {
        _width = width; _length = length; _sx = sx; _sy = sy;
        _diagonal = Math.Sqrt(_sx * _sx + _sy * _sy);
        _cellArea = _sx * _sy;
        int count = width * length;
        Receiver = new int[count]; Order = new int[count];
        Slope = new double[count]; Area = new double[count];
        _incoming = new int[count];
    }

    public void Build(float[] heights)
    {
        Array.Clear(_incoming);
        Array.Fill(Area, _cellArea);
        for (int i = 0; i < heights.Length; i++)
        {
            FindReceiver(heights, i);
            if (Receiver[i] >= 0) _incoming[Receiver[i]]++;
        }
        int tail = 0;
        for (int i = 0; i < heights.Length; i++)
            if (_incoming[i] == 0) Order[tail++] = i;
        // 接收点只有在全部上游完成后才入队；严格下降的流向图无环。
        for (int head = 0; head < tail; head++)
        {
            int i = Order[head], j = Receiver[i];
            if (j < 0) continue;
            Area[j] += Area[i];
            if (--_incoming[j] == 0) Order[tail++] = j;
        }
    }

    private void FindReceiver(float[] heights, int i)
    {
        int x = i % _width, y = i / _width;
        int best = -1;
        double steepest = 0d;
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
        {
            if (dx == 0 && dy == 0) continue;
            int nx = x + dx, ny = y + dy;
            if ((uint)nx >= (uint)_width || (uint)ny >= (uint)_length) continue;
            int j = ny * _width + nx;
            double distance = dx == 0 ? _sy : dy == 0 ? _sx : _diagonal;
            double slope = ((double)heights[i] - heights[j]) / distance;
            if (slope <= steepest) continue;
            steepest = slope;
            best = j;
        }
        Receiver[i] = best;
        Slope[i] = steepest;
    }
}