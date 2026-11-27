namespace XuanYu.Editor.TerrainEditing;

public static class TerrainHydraulicErosion
{
    public readonly record struct Result(int Iterations, float MaxCutMeters,
        float MaxFillMeters, double CutVolumeCubicMeters, double FillVolumeCubicMeters,
        double MaxDrainageAreaSquareMeters);

    public static Result Apply(float[] relief, int width, float sx, float sy, int passes)
    {
        Validate(relief, width, sx, sy, passes);
        if (passes == 0) return default;
        var before = (float[])relief.Clone();
        var solver = new TerrainHydraulicPass(width, relief.Length / width, sx, sy);
        int iterations = 0;
        double maxArea = 0d;
        for (int i = 0; i < passes; i++)
        {
            bool changed = solver.Step(relief);
            iterations++;
            maxArea = Math.Max(maxArea, solver.MaxDrainageArea);
            if (!changed) break;
        }
        return Measure(before, relief, (double)sx * sy, iterations, maxArea);
    }

    private static Result Measure(float[] before, float[] after, double cellArea,
        int iterations, double maxArea)
    {
        float maxCut = 0f, maxFill = 0f;
        double cut = 0d, fill = 0d;
        for (int i = 0; i < after.Length; i++)
        {
            float difference = after[i] - before[i];
            maxCut = MathF.Max(maxCut, -difference);
            maxFill = MathF.Max(maxFill, difference);
            cut += Math.Max(0d, (double)before[i] - after[i]);
            fill += Math.Max(0d, (double)after[i] - before[i]);
        }
        return new(iterations, maxCut, maxFill, cut * cellArea, fill * cellArea, maxArea);
    }

    private static void Validate(float[] relief, int width, float sx, float sy, int passes)
    {
        ArgumentNullException.ThrowIfNull(relief);
        if (width < 2 || relief.Length % width != 0 || relief.Length / width < 2)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (!float.IsFinite(sx) || sx <= 0f)
            throw new ArgumentOutOfRangeException(nameof(sx));
        if (!float.IsFinite(sy) || sy <= 0f)
            throw new ArgumentOutOfRangeException(nameof(sy));
        if (passes < 0) throw new ArgumentOutOfRangeException(nameof(passes));
        foreach (float h in relief)
            if (!float.IsFinite(h) || h < 0f)
                throw new ArgumentOutOfRangeException(nameof(relief));
    }
}