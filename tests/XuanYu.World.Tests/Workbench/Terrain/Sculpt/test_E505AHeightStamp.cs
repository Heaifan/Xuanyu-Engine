using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E505AHeightStamp
{
    [Fact]
    public void RaiseAndLower_ModifyRealHeightSamples()
    {
        var field = new TerrainHeightField(21, 21, 1f);
        var brush = new TerrainBrushSettings(4f, 0.5f, 1f);
        var center = new Vector2(10f, 10f);
        Assert.True(TerrainHeightStamp.Apply(field, center, brush, false) > 0);
        Assert.Equal(0.5f, field.GetHeight(10, 10));
        Assert.Equal(0.25f, field.GetHeight(12, 10));
        Assert.Equal(0f, field.GetHeight(14, 10));
        TerrainHeightStamp.Apply(field, center, brush, true);
        Assert.All(field.Samples.ToArray(), height => Assert.Equal(0f, height));
    }

    [Fact]
    public void DifferentAxisSpacing_UsesMetersAndClipsAtBorder()
    {
        var field = new TerrainHeightField(8, 8, 2f, 1f);
        var brush = new TerrainBrushSettings(4f, 1f, 1f);
        TerrainHeightStamp.Apply(field, Vector2.Zero, brush, false);
        Assert.Equal(1f, field.GetHeight(0, 0));
        Assert.Equal(0.5f, field.GetHeight(1, 0));
        Assert.Equal(field.GetHeight(1, 0), field.GetHeight(0, 2));
        Assert.Equal(0f, field.GetHeight(2, 0));
        Assert.Equal(0f, field.GetHeight(0, 4));
    }

    [Fact]
    public void ZeroStrength_DoesNotChangeExistingHeight()
    {
        var field = new TerrainHeightField(5, 5, 1f);
        field.SetHeight(2, 2, 7f);
        var brush = new TerrainBrushSettings(4f, 0f, 1f);
        Assert.Equal(0, TerrainHeightStamp.Apply(field, new Vector2(2f), brush, false));
        Assert.Equal(7f, field.GetHeight(2, 2));
    }
}