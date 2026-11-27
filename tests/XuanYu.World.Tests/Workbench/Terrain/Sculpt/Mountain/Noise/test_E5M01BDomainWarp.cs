using Xunit;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M01BDomainWarp
{
    [Fact]
    public void ZeroAmplitude_PreservesCoordinates()
    {
        var warp = new TerrainDomainWarp(7, 100d, 4, 0d);
        Assert.Equal((-37.5d, 81.25d), warp.Warp(-37.5d, 81.25d));
    }

    [Fact]
    public void Displacement_IsBoundedAndNotConstant()
    {
        var warp = new TerrainDomainWarp(7, 100d, 4, 30d);
        double maxX = 0d, maxY = 0d, axisDifference = 0d;
        for (int y = -10; y <= 10; y++)
        for (int x = -10; x <= 10; x++)
        {
            double px = x * 13d, py = y * 17d;
            var p = warp.Warp(px, py);
            double dx = p.X - px, dy = p.Y - py;
            Assert.InRange(dx, -30d, 30d);
            Assert.InRange(dy, -30d, 30d);
            maxX = Math.Max(maxX, Math.Abs(dx));
            maxY = Math.Max(maxY, Math.Abs(dy));
            axisDifference = Math.Max(axisDifference, Math.Abs(dx - dy));
        }
        Assert.True(maxX > 0.1d && maxY > 0.1d && axisDifference > 0.1d);
    }

    [Fact]
    public void SeedAndPhysicalScale_AreDeterministic()
    {
        var a = new TerrainDomainWarp(7, 80d, 4, 20d);
        var b = new TerrainDomainWarp(7, 160d, 4, 40d);
        var repeat = new TerrainDomainWarp(7, 80d, 4, 20d);
        var other = new TerrainDomainWarp(8, 80d, 4, 20d);
        for (int i = -10; i <= 10; i++)
        {
            var p = a.Warp(i * 13d, 29d);
            Assert.Equal(p, repeat.Warp(i * 13d, 29d));
            Assert.Equal((p.X * 2d, p.Y * 2d), b.Warp(i * 26d, 58d));
        }
        Assert.Contains(Enumerable.Range(0, 32), i =>
            Math.Abs(a.Warp(i * 13d, 29d).X - other.Warp(i * 13d, 29d).X) > 0.01d);
    }

    [Fact]
    public void WarpedRidgedField_IsBoundedAndDifferent()
    {
        var warp = new TerrainDomainWarp(7, 80d, 4, 30d);
        var ridge = new TerrainFractalNoise(91, 100d, 5);
        double difference = 0d;
        for (int i = -20; i <= 20; i++)
        {
            double x = i * 11d, y = 37d;
            var p = warp.Warp(x, y);
            double value = ridge.SampleRidged(p.X, p.Y);
            Assert.InRange(value, 0d, 1d);
            difference = Math.Max(difference, Math.Abs(value - ridge.SampleRidged(x, y)));
        }
        Assert.True(difference > 0.01d);
    }

    [Fact]
    public void InvalidSettings_AreRejectedAtConstruction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainDomainWarp(7, 80d, 4, -1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainDomainWarp(7, 80d, 4, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainDomainWarp(7, 0d, 4, 10d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainDomainWarp(7, 80d, 13, 10d));
    }
}