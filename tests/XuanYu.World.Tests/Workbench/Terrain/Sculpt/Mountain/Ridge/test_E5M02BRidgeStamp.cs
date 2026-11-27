using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M02BRidgeStamp
{
    [Fact]
    public void StraightRidge_IsContinuousAndBoundedInMeters()
    {
        var (field, stamp) = Create();
        Assert.True(stamp.Apply(new(20, 40), new(60, 40)) > 0);
        Assert.Equal(40f, field.GetHeight(20, 20));
        for (int x = 20; x <= 60; x++)
            Assert.InRange(field.GetHeight(x, 20), 0.001f, 40f);
        Assert.Equal(0f, field.GetHeight(40, 28));
        Assert.Equal(0f, field.GetHeight(76, 20));
        Assert.Equal(0f, field.GetHeight(4, 20));
        Assert.All(field.Samples.ToArray(), h => Assert.InRange(h, 0f, 40f));
    }

    [Fact]
    public void CollinearSubdivisions_ProduceSameSurface()
    {
        var (a, whole) = Create(); var (b, parts) = Create();
        whole.Apply(new(20, 40), new(60, 40));
        for (int x = 20; x < 60; x += 2)
            parts.Apply(new(x, 40), new(x + 2, 40));
        for (int i = 0; i < a.SampleCount; i++)
            Assert.InRange(MathF.Abs(a.Samples[i] - b.Samples[i]), 0f, 0.0001f);
    }

    [Fact]
    public void RepeatedAndReversedSegment_DoesNotRaiseAgain()
    {
        var (field, stamp) = Create();
        stamp.Apply(new(20, 40), new(60, 40));
        var before = field.Samples.ToArray();
        Assert.Equal(0, stamp.Apply(new(20, 40), new(60, 40)));
        stamp.Apply(new(60, 40), new(20, 40));
        for (int i = 0; i < before.Length; i++)
            Assert.InRange(MathF.Abs(before[i] - field.Samples[i]), 0f, 0.0001f);
    }

    [Fact]
    public void BentPath_UsesReferenceHeightAndLeavesOutsideUnchanged()
    {
        var field = new TerrainHeightField(81, 41, 1f, 2f);
        var reference = new TerrainHeightField(81, 41, 1f, 2f);
        for (int y = 0; y < 41; y++)
        for (int x = 0; x < 81; x++)
        {
            field.SetHeight(x, y, x - 40f);
            reference.SetHeight(x, y, x - 40f);
        }
        var stamp = new TerrainNaturalRidgeStamp(field, reference,
            new TerrainBrushSettings(16f, 1f, 0.5f), 40f, 7429, new(20, 40));
        stamp.Apply(new(20, 40), new(40, 40));
        stamp.Apply(new(40, 40), new(40, 64));
        Assert.Equal(20f, field.GetHeight(20, 20));
        Assert.InRange(field.GetHeight(40, 28), 0.001f, 40f);
        Assert.Equal(20f, field.GetHeight(60, 30));
        for (int i = 0; i < field.SampleCount; i++)
            Assert.InRange(field.Samples[i] - reference.Samples[i], 0f, 40.0001f);
    }

    [Fact]
    public void SeedChangesShape_AndZeroLengthSegmentIsValid()
    {
        var (a, first) = Create(7); var (b, same) = Create(7); var (c, other) = Create(8);
        foreach (var stamp in new[] { first, same, other })
        {
            stamp.Apply(new(20, 40), new(20, 40));
            stamp.Apply(new(20, 40), new(60, 40));
        }
        Assert.Equal(a.Samples.ToArray(), b.Samples.ToArray());
        Assert.Contains(a.Samples.ToArray().Zip(c.Samples.ToArray()), p => p.First != p.Second);
    }

    private static (TerrainHeightField, TerrainNaturalRidgeStamp) Create(int seed = 7429)
    {
        var field = new TerrainHeightField(81, 41, 1f, 2f);
        var reference = new TerrainHeightField(81, 41, 1f, 2f);
        return (field, new TerrainNaturalRidgeStamp(field, reference,
            new TerrainBrushSettings(16f, 1f, 0.5f), 40f, seed, new(20, 40)));
    }
}