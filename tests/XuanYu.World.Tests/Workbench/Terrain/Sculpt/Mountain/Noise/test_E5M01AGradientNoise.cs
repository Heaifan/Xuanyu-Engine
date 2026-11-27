using Xunit;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M01AGradientNoise
{
    [Fact]
    public void SameSeed_ProducesSameSamplesRegardlessOfOrder()
    {
        var a = new TerrainGradientNoise(7429);
        var b = new TerrainGradientNoise(7429);
        double first = a.Sample(1.37d, -2.18d);
        double second = a.Sample(-7.1d, 3.9d);
        Assert.Equal(second, b.Sample(-7.1d, 3.9d));
        Assert.Equal(first, b.Sample(1.37d, -2.18d));
        Assert.Equal(first, a.Sample(1.37d, -2.18d));
    }

    [Fact]
    public void DifferentSeeds_ChangeTheField()
    {
        var a = new TerrainGradientNoise(7);
        var b = new TerrainGradientNoise(8);
        Assert.Contains(Enumerable.Range(0, 32), i =>
            Math.Abs(a.Sample(i * 0.37d, -1.23d) - b.Sample(i * 0.37d, -1.23d)) > 0.01d);
    }

    [Fact]
    public void LatticePoints_AreZeroIncludingNegativeCoordinates()
    {
        var noise = new TerrainGradientNoise(-17);
        for (int y = -4; y <= 4; y++)
        for (int x = -4; x <= 4; x++)
            Assert.Equal(0d, noise.Sample(x, y));
    }

    [Fact]
    public void CellBoundaries_AreContinuousInBothAxes()
    {
        var noise = new TerrainGradientNoise(91);
        const double e = 0.00001d;
        for (int i = -4; i <= 4; i++)
        {
            Assert.InRange(Math.Abs(noise.Sample(i - e, 0.37d) - noise.Sample(i + e, 0.37d)), 0d, 0.0001d);
            Assert.InRange(Math.Abs(noise.Sample(0.37d, i - e) - noise.Sample(0.37d, i + e)), 0d, 0.0001d);
        }
    }

    [Fact]
    public void Field_IsBoundedAndNotConstant()
    {
        var noise = new TerrainGradientNoise(0);
        double min = 1d, max = -1d;
        for (int y = -32; y <= 32; y++)
        for (int x = -32; x <= 32; x++)
        {
            double value = noise.Sample(x * 0.137d, y * 0.173d);
            Assert.InRange(value, -1d, 1d);
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }
        Assert.True(min < -0.05d && max > 0.05d);
    }
}