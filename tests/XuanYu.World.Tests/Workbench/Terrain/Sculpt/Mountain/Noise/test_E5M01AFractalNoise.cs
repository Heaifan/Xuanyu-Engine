using Xunit;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M01AFractalNoise
{
    [Fact]
    public void OneOctave_MatchesGradientAtMeterScaledCoordinates()
    {
        var gradient = new TerrainGradientNoise(7);
        var fractal = new TerrainFractalNoise(7, 100d, 1);
        Assert.Equal(gradient.Sample(0.25d, -0.75d), fractal.Sample(25d, -75d));
    }

    [Fact]
    public void Wavelength_ScalesPhysicalCoordinates()
    {
        var a = new TerrainFractalNoise(91, 80d, 6);
        var b = new TerrainFractalNoise(91, 160d, 6);
        for (int i = -10; i <= 10; i++)
            Assert.Equal(a.Sample(i * 13d, 29d), b.Sample(i * 26d, 58d));
    }

    [Fact]
    public void Octaves_AddDetailWithinNormalizedRange()
    {
        var coarse = new TerrainFractalNoise(7, 100d, 1);
        var fine = new TerrainFractalNoise(7, 100d, 6);
        var repeat = new TerrainFractalNoise(7, 100d, 6);
        double difference = 0d;
        for (int y = -16; y <= 16; y++)
        for (int x = -16; x <= 16; x++)
        {
            double value = fine.Sample(x * 7d, y * 9d);
            Assert.InRange(value, -1d, 1d);
            Assert.Equal(value, repeat.Sample(x * 7d, y * 9d));
            difference = Math.Max(difference, Math.Abs(value - coarse.Sample(x * 7d, y * 9d)));
        }
        Assert.True(difference > 0.02d);
    }

    [Fact]
    public void InvalidSettings_AreRejectedAtConstruction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainFractalNoise(7, 0d, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainFractalNoise(7, double.NaN, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainFractalNoise(7, 100d, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainFractalNoise(7, 100d, 13));
    }
}