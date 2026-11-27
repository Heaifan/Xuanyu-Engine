using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M03B02RainStamp
{
    [Theory]
    [InlineData(1f, 1f, 30f, 30f)]
    [InlineData(1f, 2f, 30f, 30f)]
    [InlineData(1f, 1f, 2f, 2f)]
    public void Stamp_ConservesAndDoesNotWriteOutsideCircle(float sx, float sy, float cx, float cy)
    {
        var field = CreateSlope(sx, sy);
        var repeat = CreateSlope(sx, sy);
        var before = field.Samples.ToArray();
        var brush = new TerrainBrushSettings(12f, 0.5f, 0.5f);
        var center = new Vector2(cx, cy);
        Assert.True(TerrainRainStamp.Apply(field, center, brush, 3000, 17) > 0);
        TerrainRainStamp.Apply(repeat, center, brush, 3000, 17);
        var after = field.Samples.ToArray();
        Assert.Equal(after, repeat.Samples.ToArray());
        Assert.Contains(after.Zip(before), p => p.First < p.Second);
        Assert.Contains(after.Zip(before), p => p.First > p.Second);
        Assert.All(after, z => Assert.True(float.IsFinite(z)));
        Assert.InRange(Math.Abs(after.Sum(z => (double)z) - before.Sum(z => (double)z)) * sx * sy,
            0d, 0.02d * sx * sy);
        for (int y = 0; y < field.SamplesY; y++)
        for (int x = 0; x < field.SamplesX; x++)
        {
            if (Vector2.Distance(center, new Vector2(x * sx, y * sy)) >= brush.RadiusMeters ||
                x == 0 || y == 0 || x == field.SamplesX - 1 || y == field.SamplesY - 1)
                Assert.Equal(before[y * field.SamplesX + x], field.GetHeight(x, y));
        }
    }

    [Fact]
    public void FlatZeroStrengthAndZeroRain_DoNotChangeTerrain()
    {
        var flat = new TerrainHeightField(31, 31, 1f);
        var brush = new TerrainBrushSettings(10f, 1f, 0.5f);
        Assert.Equal(0, TerrainRainStamp.Apply(flat, new(15, 15), brush, 3000, 17));
        Assert.All(flat.Samples.ToArray(), h => Assert.Equal(0f, h));
        var slope = CreateSlope(1f, 1f);
        var before = slope.Samples.ToArray();
        Assert.Equal(0, TerrainRainStamp.Apply(slope, new(15, 15), brush, 0, 17));
        Assert.Equal(0, TerrainRainStamp.Apply(slope, new(15, 15),
            new TerrainBrushSettings(10f, 0f, 0.5f), 3000, 17));
        Assert.Equal(before, slope.Samples.ToArray());
    }

    internal static TerrainHeightField CreateSlope(float sx, float sy)
    {
        var field = new TerrainHeightField(81, 61, sx, sy);
        for (int y = 0; y < field.SamplesY; y++)
        for (int x = 0; x < field.SamplesX; x++)
            field.SetHeight(x, y, 10f - x * sx * 0.4f - y * sy * 0.2f);
        return field;
    }
}