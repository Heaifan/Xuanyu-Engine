namespace XuanYu.Editor.TerrainEditing;

public static class TerrainRainErosion
{
    public const float DefaultTravelMeters = 34f;
    public const int MaxMovingSteps = 136;
    public readonly record struct Result(int Droplets, int MovingSteps,
        float MaxCutMeters, float MaxFillMeters, double CutVolumeCubicMeters,
        double FillVolumeCubicMeters);

    public static Result Apply(float[] heights, int width, float sx, float sy,
        int droplets, float strength, int seed) =>
        Apply(heights, width, sx, sy, droplets, strength, seed, null);

    public static Result Apply(float[] heights, int width, float sx, float sy,
        int droplets, float strength, int seed, float travelMeters) =>
        Apply(heights, width, sx, sy, droplets, strength, seed, null, travelMeters);

    internal static Result Apply(float[] heights, int width, float sx, float sy,
        int droplets, float strength, int seed, float[]? weights,
        float travelMeters = DefaultTravelMeters)
    {
        Validate(heights, width, sx, sy, droplets, strength, travelMeters);
        if (droplets == 0 || strength == 0f) return default;
        var grid = new TerrainRainGrid(heights, width, sx, sy, weights);
        int limit = (int)Math.Clamp(Math.Floor(travelMeters / grid.Step), 1d, MaxMovingSteps);
        double retention = Math.Pow(0.975d, grid.Step);
        var random = new Random(seed);
        int steps = 0;
        for (int i = 0; i < droplets; i++)
        {
            double x = 1 + random.NextDouble() * (width - 3);
            double y = 1 + random.NextDouble() * (grid.Height - 3);
            if (!grid.Contains(x, y)) continue;
            steps += RunDrop(grid, x, y, strength, limit, retention);
        }
        return grid.Commit(droplets, steps);
    }

    private static int RunDrop(TerrainRainGrid grid, double x, double y, float strength,
        int limit, double retention)
    {
        double dx = 0, dy = 0, speed = 1, water = 1, load = 0;
        int steps = 0;
        for (; steps < limit && water > 0.02; steps++)
        {
            var sample = grid.Sample(x, y);
            dx = dx * 0.08 - sample.Gx * 0.92;
            dy = dy * 0.08 - sample.Gy * 0.92;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 1e-8) break;
            dx /= length;
            dy /= length;
            double nx = x + dx * grid.Step / grid.Sx;
            double ny = y + dy * grid.Step / grid.Sy;
            if (!grid.Contains(nx, ny)) break;
            double drop = sample.H - grid.Sample(nx, ny).H;
            if (drop <= 0) break;
            double capacity = drop * speed * water * 4.2;
            if (load > capacity)
            {
                double amount = (load - capacity) * 0.32;
                grid.Deposit(x, y, amount);
                load -= amount;
            }
            else
            {
                double amount = Math.Min((capacity - load) * 0.28, drop * 0.5) * strength;
                load += grid.Erode(x, y, amount);
            }
            speed = Math.Min(4, Math.Sqrt(speed * speed + drop / grid.Step * 4));
            water *= retention;
            x = nx;
            y = ny;
        }
        grid.Settle(x, y, load);
        return steps;
    }

    private static void Validate(float[] heights, int width, float sx, float sy,
        int droplets, float strength, float travelMeters)
    {
        ArgumentNullException.ThrowIfNull(heights);
        if (width < 4 || heights.Length % width != 0 || heights.Length / width < 4)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (!float.IsFinite(sx) || sx <= 0f) throw new ArgumentOutOfRangeException(nameof(sx));
        if (!float.IsFinite(sy) || sy <= 0f) throw new ArgumentOutOfRangeException(nameof(sy));
        if (!float.IsFinite(travelMeters) || travelMeters <= 0f)
            throw new ArgumentOutOfRangeException(nameof(travelMeters));
        if (droplets < 0) throw new ArgumentOutOfRangeException(nameof(droplets));
        if (!float.IsFinite(strength) || strength < 0f || strength > 1f)
            throw new ArgumentOutOfRangeException(nameof(strength));
        foreach (float h in heights)
            if (!float.IsFinite(h)) throw new ArgumentOutOfRangeException(nameof(heights));
    }
}