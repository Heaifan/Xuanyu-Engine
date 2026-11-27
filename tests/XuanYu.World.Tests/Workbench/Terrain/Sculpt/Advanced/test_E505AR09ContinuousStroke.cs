using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E505AR09ContinuousStroke
{
    [Fact]
    public void AreaStroke_ContinuesAroundCornerWithoutWaitingForTime()
    {
        var field = new TerrainHeightField(51, 51, 1f);
        var brush = new TerrainBrushSettings(6f, 1f, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, false, 100f, true);
        stroke.MoveTo(new(5, 5));
        stroke.MoveTo(new(5, 35));
        stroke.MoveTo(new(35, 35));
        Assert.True(stroke.Advance(0f) > 0);
        for (int i = 5; i <= 35; i++)
        {
            Assert.Equal(100f, field.GetHeight(5, i));
            Assert.Equal(100f, field.GetHeight(i, 35));
        }
        Assert.Equal(0f, field.GetHeight(20, 20));
        Assert.Equal(0, stroke.Advance(1f));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SculptStroke_PreservesCurvedPathBetweenTicks(bool lower)
    {
        var field = new TerrainHeightField(51, 51, 1f);
        var stroke = new TerrainSculptStroke(field, new TerrainBrushSettings(6f, 1f, 1f), lower);
        stroke.MoveTo(new(5, 5));
        stroke.MoveTo(new(5, 35));
        stroke.MoveTo(new(35, 35));
        Assert.True(stroke.Advance(1f) > 0);
        float sign = lower ? -1f : 1f;
        Assert.True(sign * field.GetHeight(5, 20) > 0f);
        Assert.True(sign * field.GetHeight(20, 35) > 0f);
        Assert.Equal(0f, field.GetHeight(20, 20));
    }

    [Fact]
    public void AreaStroke_MissPreservesPendingPathWithoutBridgingGap()
    {
        var field = new TerrainHeightField(61, 21, 1f);
        var stroke = new TerrainSculptStroke(field, new TerrainBrushSettings(4f, 1f, 0f), false, 100f, true);
        stroke.MoveTo(new(5, 10));
        stroke.MoveTo(new(15, 10));
        stroke.BreakPath();
        stroke.MoveTo(new(45, 10));
        stroke.MoveTo(new(55, 10));
        stroke.Advance(0f);
        Assert.Equal(100f, field.GetHeight(10, 10));
        Assert.Equal(100f, field.GetHeight(55, 10));
        Assert.Equal(0f, field.GetHeight(30, 10));
    }

    [Fact]
    public void PointElevation_RemainsSingleStampPerStroke()
    {
        var field = new TerrainHeightField(41, 21, 1f);
        var stroke = new TerrainSculptStroke(field, new TerrainBrushSettings(4f, 1f, 1f), false, 100f);
        stroke.MoveTo(new(5, 10));
        stroke.MoveTo(new(35, 10));
        stroke.Advance(1f);
        Assert.Equal(100f, field.GetHeight(5, 10));
        Assert.Equal(0f, field.GetHeight(35, 10));
    }
}