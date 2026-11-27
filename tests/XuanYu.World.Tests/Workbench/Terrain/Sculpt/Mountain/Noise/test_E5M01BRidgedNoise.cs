using Xunit;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M01BRidgedNoise
{
    [Fact]
    public void SingleOctave_FoldsGradientIntoRidge()
    {
        var gradient = new TerrainGradientNoise(7);
        var ridged = new TerrainFractalNoise(7, 100d, 1);
        Assert.Equal(1d, ridged.SampleRidged(0d, 0d));
        double noise = gradient.Sample(0.25d, -0.75d);
        double crest = 1d - Math.Abs(noise);
        Assert.Equal(crest * crest, ridged.SampleRidged(25d, -75d));
    }

    [Fact]
    public void MultiOctave_IsBoundedAndAddsDetail()
    {
        var coarse = new TerrainFractalNoise(7, 100d, 1);
        var fine = new TerrainFractalNoise(7, 100d, 6);
        double difference = 0d, min = 1d;
        for (int y = -16; y <= 16; y++)
        for (int x = -16; x <= 16; x++)
        {
            double value = fine.SampleRidged(x * 7d, y * 9d);
            Assert.InRange(value, 0d, 1d);
            min = Math.Min(min, value);
            difference = Math.Max(difference, Math.Abs(value - coarse.SampleRidged(x * 7d, y * 9d)));
        }
        Assert.True(min < 0.9d);
        Assert.True(difference > 0.01d);
    }

    [Fact]
    public void SeedAndWavelength_ControlTheFieldDeterministically()
    {
        var a = new TerrainFractalNoise(91, 80d, 5);
        var b = new TerrainFractalNoise(91, 160d, 5);
        var other = new TerrainFractalNoise(92, 80d, 5);
        for (int i = -10; i <= 10; i++)
            Assert.Equal(a.SampleRidged(i * 13d, 29d), b.SampleRidged(i * 26d, 58d));
        Assert.Contains(Enumerable.Range(0, 32), i =>
            Math.Abs(a.SampleRidged(i * 13d, 29d) - other.SampleRidged(i * 13d, 29d)) > 0.01d);
    }

    [Fact]
    public void MultiOctave_RemainsContinuousAcrossBaseLattice()
    {
        var noise = new TerrainFractalNoise(7, 100d, 6);
        const double e = 0.00001d;
        for (int i = -4; i <= 4; i++)
        {
            double p = i * 100d;
            Assert.InRange(Math.Abs(noise.SampleRidged(p - e, 37d) -
                noise.SampleRidged(p + e, 37d)), 0d, 0.0001d);
            Assert.InRange(Math.Abs(noise.SampleRidged(37d, p - e) -
                noise.SampleRidged(37d, p + e)), 0d, 0.0001d);
        }
    }
}