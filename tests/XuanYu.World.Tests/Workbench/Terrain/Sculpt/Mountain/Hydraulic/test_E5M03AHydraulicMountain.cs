using System.Diagnostics;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M03AHydraulicMountain
{
    private readonly ITestOutputHelper _output;
    public test_E5M03AHydraulicMountain(ITestOutputHelper output) => _output = output;

    [Theory]
    [InlineData(false, 1f)]
    [InlineData(true, 5f)]
    public void Mountain_ErodesAndDepositsDeterministically(bool ridge, float spacing)
    {
        var field = new TerrainHeightField(101, 101, spacing);
        var brush = new TerrainBrushSettings(15f * spacing, 0f, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, false, naturalPeakHeight: 20f * spacing);
        stroke.MoveTo(new Vector2(30, 50) * spacing);
        if (ridge)
        {
            stroke.MoveTo(new Vector2(70, 50) * spacing);
            stroke.Advance(0f);
        }
        var before = field.Samples.ToArray();
        var after = (float[])before.Clone();
        var repeat = (float[])before.Clone();
        var watch = Stopwatch.StartNew();
        var result = TerrainHydraulicErosion.Apply(after, field.SamplesX, spacing, spacing, 16);
        watch.Stop();
        var second = TerrainHydraulicErosion.Apply(repeat, field.SamplesX, spacing, spacing, 16);
        Assert.InRange(result.Iterations, 1, 16);
        Assert.True(result.MaxCutMeters > 0.01f);
        Assert.True(result.MaxFillMeters > 0.01f);
        Assert.True(result.MaxDrainageAreaSquareMeters > (double)spacing * spacing);
        Assert.All(after, h => Assert.True(float.IsFinite(h) && h >= 0f));
        Assert.Equal(after, repeat);
        Assert.Equal(result, second);
        double initialVolume = before.Sum(h => (double)h) * spacing * spacing;
        double finalVolume = after.Sum(h => (double)h) * spacing * spacing;
        Assert.InRange(Math.Abs(finalVolume - initialVolume), 0d, initialVolume * 0.00001d);
        Assert.InRange(Math.Abs(result.CutVolumeCubicMeters - result.FillVolumeCubicMeters),
            0d, initialVolume * 0.00001d);
        _output.WriteLine($"[E5-M03A] { (ridge ? "山脉" : "山峰") } " +
            $"迭代={result.Iterations}，削低={result.MaxCutMeters:0.###}m，" +
            $"堆积={result.MaxFillMeters:0.###}m，" +
            $"削方={result.CutVolumeCubicMeters:0.###}m³，" +
            $"填方={result.FillVolumeCubicMeters:0.###}m³，" +
            $"最大汇水面积={result.MaxDrainageAreaSquareMeters:0.###}m²，" +
            $"计算={watch.Elapsed.TotalMilliseconds:0.##}ms");
    }
}