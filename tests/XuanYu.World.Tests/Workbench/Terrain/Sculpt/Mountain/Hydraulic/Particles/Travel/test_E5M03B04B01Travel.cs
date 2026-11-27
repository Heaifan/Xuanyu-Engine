using System.Diagnostics;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M03B04B01Travel(ITestOutputHelper output)
{
    [Theory]
    [InlineData(0.5f, 1f)]
    [InlineData(0.25f, 0.5f)]
    public void FineGrid_LongTravelUsesMoreStepsAndConserves(float sx, float sy)
    {
        const int w = 321, h = 17, count = 64;
        var before = Enumerable.Range(0, w * h).Select(i => 20f - i % w * sx * 0.4f).ToArray();
        var shortRun = (float[])before.Clone();
        var longRun = (float[])before.Clone();
        var repeat = (float[])before.Clone();
        var a = TerrainRainErosion.Apply(shortRun, w, sx, sy, count, 0.001f, 17, 34f * sx);
        var b = TerrainRainErosion.Apply(longRun, w, sx, sy, count, 0.001f, 17);
        var c = TerrainRainErosion.Apply(repeat, w, sx, sy, count, 0.001f, 17);
        Assert.True(a.MovingSteps > 0);
        Assert.InRange(a.MovingSteps, 1, count * 34);
        Assert.True(b.MovingSteps > a.MovingSteps);
        Assert.InRange(b.MovingSteps, 1, count * TerrainRainErosion.MaxMovingSteps);
        Assert.Equal(b, c); Assert.Equal(longRun, repeat);
        AssertSurface(before, longRun, w, sx, sy);
        output.WriteLine($"[E5-M03B-04B01] 间距={sx}×{sy}m，短行程步数={a.MovingSteps}，米制行程步数={b.MovingSteps}");
    }

    [Fact]
    public void TravelBudget_RejectsInvalidAndCapsFineGridWork()
    {
        const int w = 321, h = 9;
        var heights = Enumerable.Range(0, w * h).Select(i => 30f - i % w * 0.02f).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TerrainRainErosion.Apply(heights, w, 0.1f, 0.1f, 1, 0.5f, 17, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TerrainRainErosion.Apply(heights, w, 0.1f, 0.1f, 1, 0.5f, 17, 0f));
        var a = TerrainRainErosion.Apply(heights, w, 0.1f, 0.1f, 64, 0.001f, 17);
        Assert.InRange(a.MovingSteps, 1, 64 * TerrainRainErosion.MaxMovingSteps);
    }

    [Fact]
    public void SameNaturalPeak_CompareTravelAndD8()
    {
        const int w = 161;
        const float s = 0.5f;
        var field = new TerrainHeightField(w, w, s);
        new TerrainSculptStroke(field, new TerrainBrushSettings(25f, 0.5f, 0.5f), false,
            naturalPeakHeight: 30f, naturalPeakSeed: 7429).MoveTo(new Vector2(40, 40));
        var before = field.Samples.ToArray();
        foreach (float meters in new[] { 17f, 34f })
        {
            var after = (float[])before.Clone(); var watch = Stopwatch.StartNew();
            var result = TerrainRainErosion.Apply(after, w, s, s, 9000, 0.5f, 17, meters);
            watch.Stop(); AssertSurface(before, after, w, s, s);
            Report($"雨滴 {meters}m，步数={result.MovingSteps}", before, after, watch.Elapsed.TotalMilliseconds);
        }
        var d8 = (float[])before.Clone(); var timer = Stopwatch.StartNew();
        TerrainHydraulicErosion.Apply(d8, w, s, s, 16);
        timer.Stop();
        Report("D8 16轮", before, d8, timer.Elapsed.TotalMilliseconds);
    }

    private void Report(string label, float[] before, float[] after, double ms)
    {
        var pairs = before.Zip(after).ToArray();
        double upper = pairs.Where(p => p.First >= 10f).Sum(p => Math.Max(0d, p.First - (double)p.Second));
        double cut = pairs.Sum(p => Math.Max(0d, p.First - (double)p.Second));
        double fill = pairs.Sum(p => Math.Max(0d, p.Second - (double)p.First));
        output.WriteLine($"[E5-M03B-04B01] {label}，上部削方={upper * 0.25:0.###}m³，" +
            $"总削方={cut * 0.25:0.###}m³，填方={fill * 0.25:0.###}m³，耗时={ms:0.##}ms");
    }

    private static void AssertSurface(float[] before, float[] after, int w, float sx, float sy)
    {
        Assert.All(after, z => Assert.True(float.IsFinite(z)));
        Assert.Contains(after.Zip(before), p => p.First < p.Second);
        Assert.Contains(after.Zip(before), p => p.First > p.Second);
        Assert.InRange(Math.Abs(after.Sum(z => (double)z) - before.Sum(z => (double)z)) * sx * sy,
            0d, 0.05d * sx * sy);
        for (int i = 0; i < after.Length; i++)
            if (i % w == 0 || i % w == w - 1 || i < w || i >= after.Length - w)
                Assert.Equal(before[i], after[i]);
    }
}