namespace XuanYu.Editor.TerrainEditing;

internal sealed partial class TerrainRainGrid
{
    public void Settle(double x, double y, double amount)
    {
        if (amount == 0d) return;
        Span<(int Index, double Weight)> targets = stackalloc (int, double)[25];
        int count = CollectSettlingTargets(x, y, targets);
        var active = targets[..count];
        // Contains 保证至少一个双线性有效点；5×5 邻域包含全部四个点。
        double bottom = _heights[active[0].Index];
        double level = FindSettlingLevel(active, bottom, amount);
        foreach (var (i, weight) in active)
            _heights[i] += Math.Max(0d, level - (_heights[i] - bottom)) * weight;
    }

    private int CollectSettlingTargets(double x, double y,
        Span<(int Index, double Weight)> targets)
    {
        int cx = (int)(x + 0.5), cy = (int)(y + 0.5), count = 0;
        for (int oy = -2; oy <= 2; oy++)
        for (int ox = -2; ox <= 2; ox++)
        {
            int ix = cx + ox, iy = cy + oy;
            if (ix < 1 || iy < 1 || ix >= _width - 1 || iy >= Height - 1) continue;
            int i = iy * _width + ix;
            double dx = ix - x, dy = iy - y;
            double weight = (1d - (dx * dx + dy * dy) / 6.25d) * (_weights?[i] ?? 1f);
            if (weight <= 0d) continue;
            int pos = count++;
            while (pos > 0 && _heights[targets[pos - 1].Index] > _heights[i])
            {
                targets[pos] = targets[pos - 1];
                pos--;
            }
            targets[pos] = (i, weight);
        }
        return count;
    }

    private double FindSettlingLevel(ReadOnlySpan<(int Index, double Weight)> targets,
        double bottom, double amount)
    {
        double level = 0d, totalWeight = targets[0].Weight;
        // 逐级填充低处，预算不足以到达下一高程时，只分配给已参与的低处。
        for (int k = 1; k < targets.Length; k++)
        {
            double next = _heights[targets[k].Index] - bottom;
            double cost = (next - level) * totalWeight;
            if (amount <= cost) return level + amount / totalWeight;
            amount -= cost;
            level = next;
            totalWeight += targets[k].Weight;
        }
        return level + amount / totalWeight;
    }
}