namespace XuanYu.Editor.TerrainEditing;

internal sealed class TerrainHydraulicPass
{
    private readonly int _width, _length;
    private readonly double _settleBand;
    private readonly TerrainHydraulicFlow _flow;
    private readonly double[] _sediment, _delta;
    public double MaxDrainageArea { get; private set; }

    public TerrainHydraulicPass(int width, int length, float sx, float sy)
    {
        _width = width; _length = length;
        _settleBand = Math.Min(sx, sy) * 0.25d;
        _flow = new(width, length, sx, sy);
        _sediment = new double[width * length];
        _delta = new double[width * length];
    }

    public bool Step(float[] heights)
    {
        _flow.Build(heights);
        Array.Clear(_sediment);
        Array.Clear(_delta);
        MaxDrainageArea = 0d;
        foreach (int i in _flow.Order)
        {
            MaxDrainageArea = Math.Max(MaxDrainageArea, _flow.Area[i]);
            Transport(heights, i);
        }
        bool changed = false;
        for (int i = 0; i < heights.Length; i++)
        {
            float next = (float)(heights[i] + _delta[i]);
            if (next == heights[i]) continue;
            heights[i] = next;
            changed = true;
        }
        return changed;
    }

    private void Transport(float[] heights, int i)
    {
        int j = _flow.Receiver[i];
        double load = _sediment[i];
        if (j < 0)
        {
            if (load > 0d) Settle(heights, i, load);
            return;
        }
        double drop = (double)heights[i] - heights[j];
        // 输沙量用等效高程表示；面积开方为米，坡度无量纲。
        double capacity = Math.Min(2d * drop,
            0.12d * Math.Sqrt(_flow.Area[i]) * _flow.Slope[i]);
        if (load < capacity)
        {
            double cut = Math.Min(heights[i], Math.Min(drop * 0.25d,
                (capacity - load) * 0.35d));
            _delta[i] -= cut;
            load += cut;
        }
        else
        {
            _delta[i] += load - capacity;
            load = capacity;
        }
        _sediment[j] += load;
    }

    private void Settle(float[] heights, int i, double load)
    {
        int x = i % _width, y = i / _width;
        Span<int> targets = stackalloc int[9];
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
        {
            int nx = x + dx, ny = y + dy;
            if ((uint)nx >= (uint)_width || (uint)ny >= (uint)_length) continue;
            int j = ny * _width + nx;
            if ((double)heights[j] - heights[i] <= _settleBand) targets[count++] = j;
        }
        // 终点自身必在目标中；分摊到邻近低地，避免平坦出口集中成一个点。
        double share = load / count;
        for (int k = 0; k < count; k++) _delta[targets[k]] += share;
    }
}