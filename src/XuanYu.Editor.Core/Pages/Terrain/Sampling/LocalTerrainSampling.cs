namespace XuanYu.Editor.Pages;

public readonly record struct LocalTerrainSamplePlan(
    int CellsX, int CellsY, float SpacingX, float SpacingY,
    bool Automatic);

public static class LocalTerrainSampling
{
    public static LocalTerrainSamplePlan Plan(
        float width, float length, float desiredSpacing)
    {
        if (!float.IsFinite(width) || !float.IsFinite(length) ||
            !float.IsFinite(desiredSpacing) || width <= 0f ||
            length <= 0f || desiredSpacing < 0.25f)
            throw new ArgumentOutOfRangeException(nameof(width),
                "宽度、长度须大于 0，目标间距至少 0.25m，均须是有限数字。");

        double required = Math.Max((double)width, length) / 256d;
        bool automatic = required > desiredSpacing;
        double step = automatic ? NiceSpacing(required) : desiredSpacing;
        int cellsX = Math.Clamp((int)Math.Ceiling(width / step), 1, 256);
        int cellsY = Math.Clamp((int)Math.Ceiling(length / step), 1, 256);
        return new LocalTerrainSamplePlan(cellsX, cellsY,
            width / cellsX, length / cellsY, automatic);
    }

    private static double NiceSpacing(double required)
    {
        double unit = Math.Pow(10d, Math.Floor(Math.Log10(required)));
        double factor = required / unit;
        return (factor <= 1d ? 1d : factor <= 2d ? 2d :
            factor <= 5d ? 5d : 10d) * unit;
    }
}