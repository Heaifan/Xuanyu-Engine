using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M03B01RainComparison
{
    private readonly ITestOutputHelper _output;
    public test_E5M03B01RainComparison(ITestOutputHelper output) => _output = output;

    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(1f, 2f)]
    public void SameMountain_CompareD8AndRain(float sx, float sy)
    {
        const int width = 81, height = 61;
        var original = test_E5M03B01RainErosion.MakeHill(width, height);
        var d8 = (float[])original.Clone();
        var rain = (float[])original.Clone();
        var watch = Stopwatch.StartNew();
        var a = TerrainHydraulicErosion.Apply(d8, width, sx, sy, 16);
        double d8Ms = watch.Elapsed.TotalMilliseconds;
        watch.Restart();
        var b = TerrainRainErosion.Apply(rain, width, sx, sy, 3000, 0.5f, 17);
        double rainMs = watch.Elapsed.TotalMilliseconds;
        Assert.True(a.MaxCutMeters > 0f && a.MaxFillMeters > 0f);
        Assert.True(b.MaxCutMeters > 0f && b.MaxFillMeters > 0f);
        Assert.All(d8, z => Assert.True(float.IsFinite(z)));
        Assert.All(rain, z => Assert.True(float.IsFinite(z)));
        _output.WriteLine($"[E5-M03B-01] 网格={width}×{height}，间距={sx}×{sy}m");
        _output.WriteLine($"D8：迭代={a.Iterations}，削低={a.MaxCutMeters:0.###}m，" +
            $"堆高={a.MaxFillMeters:0.###}m，削方={a.CutVolumeCubicMeters:0.###}m³，" +
            $"填方={a.FillVolumeCubicMeters:0.###}m³，耗时={d8Ms:0.###}ms");
        _output.WriteLine($"雨滴：投放={b.Droplets}，步数={b.MovingSteps}，削低={b.MaxCutMeters:0.###}m，" +
            $"堆高={b.MaxFillMeters:0.###}m，削方={b.CutVolumeCubicMeters:0.###}m³，" +
            $"填方={b.FillVolumeCubicMeters:0.###}m³，耗时={rainMs:0.###}ms");
    }
}