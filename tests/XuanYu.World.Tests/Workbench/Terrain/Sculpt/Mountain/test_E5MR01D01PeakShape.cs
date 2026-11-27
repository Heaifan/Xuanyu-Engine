using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5MR01D01PeakShape
{
    [Theory]
    [InlineData(7429)]
    [InlineData(7)]
    [InlineData(8)]
    public void Peak_MiddleSlopeHasDirectionalRelief(int seed)
    {
        var field = Create(seed, 0.5f);
        (int X, int Y)[] ring =
        [
            (40, 0), (32, 24), (24, 32), (0, 40), (-24, 32), (-32, 24),
            (-40, 0), (-32, -24), (-24, -32), (0, -40), (24, -32), (32, -24)
        ];
        var heights = ring.Select(p => field.GetHeight(100 + p.X, 100 + p.Y)).ToArray();
        Assert.All(heights, h => Assert.InRange(h, 0.01f, 59.99f));
        Assert.True(heights.Max() - heights.Min() >= 14f,
            $"种子={seed}，等距坡面高差={heights.Max() - heights.Min():0.###}m");
        Assert.Equal(60f, field.GetHeight(100, 100));
        for (int y = 0; y < field.SamplesY; y++)
        for (int x = 0; x < field.SamplesX; x++)
        {
            float h = field.GetHeight(x, y);
            Assert.InRange(h, 0f, 60f);
            int dx = x - 100, dy = y - 100;
            if (dx * dx + dy * dy >= 80 * 80) Assert.Equal(0f, h);
        }
    }

    [Fact]
    public void Peak_IncreasingFalloffNarrowsReliefWithoutChangingPeak()
    {
        var wide = Create(7429, 0f);
        var narrow = Create(7429, 1f);
        Assert.Equal(60f, wide.GetHeight(100, 100));
        Assert.Equal(60f, narrow.GetHeight(100, 100));
        double a = 0d, b = 0d;
        for (int i = 0; i < wide.SampleCount; i++)
        {
            Assert.True(narrow.Samples[i] <= wide.Samples[i] + 0.0001f);
            a += wide.Samples[i]; b += narrow.Samples[i];
        }
        Assert.True(b < a);
    }

    private static TerrainHeightField Create(int seed, float falloff)
    {
        var field = new TerrainHeightField(201, 201, 1f);
        var stroke = new TerrainSculptStroke(field, new TerrainBrushSettings(80f, 0.5f, falloff),
            false, naturalPeakHeight: 60f, naturalPeakSeed: seed);
        Assert.True(stroke.MoveTo(new Vector2(100, 100)) > 0);
        Assert.Equal(0, stroke.FinishMountain());
        return field;
    }
}