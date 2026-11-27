using Xunit;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M03B01RainErosion
{
    [Fact]
    public void FlatAndDisabledRain_DoNotChangeHeights()
    {
        var flat = Enumerable.Repeat(-15f, 41 * 31).ToArray();
        var original = (float[])flat.Clone();
        var result = TerrainRainErosion.Apply(flat, 41, 1f, 2f, 2000, 0.5f, 17);
        Assert.Equal(original, flat);
        Assert.Equal(0, result.MovingSteps);
        var terrain = MakeHill(41, 31);
        var before = (float[])terrain.Clone();
        Assert.Equal(default(TerrainRainErosion.Result),
            TerrainRainErosion.Apply(terrain, 41, 1f, 1f, 2000, 0f, 17));
        Assert.Equal(before, terrain);
    }

    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(1f, 2f)]
    public void Rain_ConservesMaterialAndIsRepeatable(float sx, float sy)
    {
        const int w = 61, h = 51;
        var before = MakeHill(w, h).Select(z => z - 20f).ToArray();
        var first = (float[])before.Clone();
        var second = (float[])before.Clone();
        var result = TerrainRainErosion.Apply(first, w, sx, sy, 3000, 0.5f, 17);
        var repeat = TerrainRainErosion.Apply(second, w, sx, sy, 3000, 0.5f, 17);
        Assert.Equal(first, second);
        Assert.Equal(result, repeat);
        Assert.True(result.MovingSteps > 0);
        Assert.True(result.MaxCutMeters > 0.01f);
        Assert.True(result.MaxFillMeters > 0.01f);
        float floor = before.Min();
        Assert.All(first, z => Assert.True(float.IsFinite(z) && z >= floor));
        Assert.InRange(Math.Abs(first.Sum(z => (double)z) - before.Sum(z => (double)z))
            * sx * sy, 0d, 0.02d * sx * sy);
        Assert.InRange(Math.Abs(result.CutVolumeCubicMeters - result.FillVolumeCubicMeters),
            0d, 0.02d * sx * sy);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            if (x == 0 || y == 0 || x == w - 1 || y == h - 1)
                Assert.Equal(before[y * w + x], first[y * w + x]);
    }

    internal static float[] MakeHill(int w, int h)
    {
        var heights = new float[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            double u = (x - (w - 1) * 0.5) / (w * 0.30);
            double v = (y - (h - 1) * 0.5) / (h * 0.30);
            heights[y * w + x] = (float)(30 * Math.Exp(-u * u - v * v)
                * (1 + 0.08 * Math.Sin(x * 0.55) * Math.Cos(y * 0.4)));
        }
        return heights;
    }
}