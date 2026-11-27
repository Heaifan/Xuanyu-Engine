using System.Diagnostics;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M03B04ASettling(ITestOutputHelper output)
{
    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(0.5f, 1f)]
    public void OneDrop_SpreadsTerminalLoadOnFlatFoot(float sx, float sy)
    {
        const int w = 9, h = 9;
        var before = Enumerable.Range(0, w * h).Select(i => i % w < 4 ? 10f : 0f).ToArray();
        int tested = 0;
        for (int seed = 0; seed < 64; seed++)
        {
            var after = (float[])before.Clone();
            var result = TerrainRainErosion.Apply(after, w, sx, sy, 1, 0.5f, seed);
            if (result.MovingSteps != 1 || result.MaxCutMeters == 0f) continue;
            tested++;
            int receiving = Enumerable.Range(0, after.Length)
                .Count(i => before[i] == 0f && after[i] > 0f);
            Assert.True(receiving > 4, $"种子={seed}，接收点={receiving}");
            Assert.InRange(Math.Abs(result.CutVolumeCubicMeters - result.FillVolumeCubicMeters), 0d, 0.0001d);
            for (int i = 0; i < after.Length; i++)
            {
                if (i % w < 4) Assert.True(after[i] <= before[i]);
                if (i % w == 0 || i % w == w - 1 || i / w == 0 || i / w == h - 1)
                    Assert.Equal(before[i], after[i]);
            }
        }
        Assert.True(tested > 0, "必须实际覆盖至少一个有载沙的单步雨滴");
    }

    [Fact]
    public void NarrowPeak_ReportsFootProminenceAndPreservesMass()
    {
        const int w = 201;
        var field = new TerrainHeightField(w, w, 0.5f);
        Vector2 center = new(50, 50);
        new TerrainSculptStroke(field, new TerrainBrushSettings(10f, 0.5f, 0.5f), false,
            naturalPeakHeight: 30f, naturalPeakSeed: 7429).MoveTo(center);
        var before = field.Samples.ToArray();
        var brush = new TerrainBrushSettings(25f, 0.5f, 0.5f);
        var watch = Stopwatch.StartNew();
        for (int i = 0; i < 90; i++) TerrainRainStamp.Apply(field, center, brush, 100, 17 + i);
        watch.Stop();
        var after = field.Samples.ToArray();
        Assert.Contains(after.Zip(before), p => p.First < p.Second);
        Assert.Contains(after.Zip(before), p => p.First > p.Second);
        Assert.InRange(Math.Abs(after.Sum(z => (double)z) - before.Sum(z => (double)z)), 0d, 0.05d);
        float max = 0f;
        int spikes = 0;
        for (int y = 1; y < w - 1; y++)
        for (int x = 1; x < w - 1; x++)
        {
            float r = Vector2.Distance(center, new Vector2(x * 0.5f, y * 0.5f));
            int i = y * w + x;
            if (r >= 25f) Assert.Equal(before[i], after[i]);
            if (r < 8f || r >= 25f) continue;
            float neighbor = Math.Max(Math.Max(after[i - 1], after[i + 1]),
                Math.Max(after[i - w], after[i + w]));
            float prominence = after[i] - neighbor;
            max = Math.Max(max, prominence);
            if (prominence > 0.25f) spikes++;
        }
        output.WriteLine($"[E5-M03B-04A] 山脚局部突起={max:0.###}m，" +
            $"突起>0.25m 样本={spikes}，90批累计耗时={watch.Elapsed.TotalMilliseconds:0.##}ms");
    }
}