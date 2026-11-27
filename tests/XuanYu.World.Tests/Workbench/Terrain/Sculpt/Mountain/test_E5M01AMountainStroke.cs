using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M01AMountainStroke
{
    [Theory]
    [InlineData(0f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    public void Peak_UsesRelativeHeightAndDoesNotAccumulateOnHold(float strength)
    {
        var field = new TerrainHeightField(41, 41, 1f);
        field.SetHeight(20, 20, 10f);
        var brush = new TerrainBrushSettings(10f, strength, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, false, naturalPeakHeight: 40f);
        Assert.True(stroke.MoveTo(new(20, 20)) > 0);
        Assert.Equal(50f, field.GetHeight(20, 20));
        Assert.InRange(field.GetHeight(25, 20), 0.01f, 39.99f);
        Assert.Equal(0f, field.GetHeight(30, 20));
        var first = field.Samples.ToArray();
        Assert.Equal(0, stroke.Advance(2f));
        Assert.Equal(0, stroke.MoveTo(new(20, 20)));
        Assert.Equal(first, field.Samples.ToArray());
        var next = new TerrainSculptStroke(field, brush, false, naturalPeakHeight: 40f);
        next.MoveTo(new(20, 20));
        Assert.Equal(90f, field.GetHeight(20, 20));
    }

    [Fact]
    public void Ridge_ContinuousAndIndependentOfStraightPathBatching()
    {
        var one = new TerrainHeightField(61, 41, 1f);
        var many = new TerrainHeightField(61, 41, 1f);
        var brush = new TerrainBrushSettings(8f, 1f, 0.5f);
        var a = new TerrainSculptStroke(one, brush, false, naturalPeakHeight: 30f);
        var b = new TerrainSculptStroke(many, brush, false, naturalPeakHeight: 30f);
        a.MoveTo(new(10, 20)); a.MoveTo(new(50, 20)); a.Advance(0f);
        b.MoveTo(new(10, 20));
        for (int x = 20; x <= 50; x += 10)
        { b.MoveTo(new(x, 20)); b.Advance(0.2f); }
        Assert.Equal(30f, one.GetHeight(10, 20));
        var crest = Enumerable.Range(10, 41).Select(x => one.GetHeight(x, 20)).ToArray();
        Assert.All(crest, z => Assert.InRange(z, 0.001f, 30.001f));
        Assert.True(crest.Max() > crest.Min());
        for (int i = 0; i < one.SampleCount; i++)
            Assert.InRange(MathF.Abs(one.Samples[i] - many.Samples[i]), 0f, 0.001f);
        var beforeReturn = one.Samples.ToArray();
        a.MoveTo(new(10, 20)); a.Advance(1f);
        for (int i = 0; i < one.SampleCount; i++)
            Assert.InRange(MathF.Abs(one.Samples[i] - beforeReturn[i]), 0f, 0.001f);
        Assert.Equal(0f, one.GetHeight(30, 28));
    }

    [Fact]
    public void BreakPath_AndIgnoredStrength_DoNotPaintUnvisitedGround()
    {
        var field = new TerrainHeightField(61, 21, 1f, 2f);
        var brush = new TerrainBrushSettings(4f, 1f, 0f);
        var stroke = new TerrainSculptStroke(field, brush, true, naturalPeakHeight: 10f);
        stroke.MoveTo(new(0, 0)); stroke.BreakPath(); stroke.MoveTo(new(50, 20));
        stroke.Advance(0.1f);
        Assert.Equal(10f, field.GetHeight(0, 0));
        Assert.InRange(field.GetHeight(50, 10), 0.001f, 10f);
        Assert.Equal(0f, field.GetHeight(25, 5));
        var zero = new TerrainSculptStroke(field, new TerrainBrushSettings(4f, 0f, 1f),
            false, naturalPeakHeight: 10f);
        Assert.True(zero.MoveTo(new(20, 20)) > 0);
        zero.MoveTo(new(30, 20)); Assert.True(zero.Advance(1f) > 0);
        Assert.Equal(10f, field.GetHeight(20, 10));
        Assert.True(field.GetHeight(30, 10) > 0f);
        Assert.Equal(0f, field.GetHeight(25, 5));
        Assert.Equal(10f, field.GetHeight(0, 0));
    }
}