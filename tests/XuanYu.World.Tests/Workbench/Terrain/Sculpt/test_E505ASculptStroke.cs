using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E505ASculptStroke
{
    [Fact]
    public void StraightStroke_IsIndependentOfPointerEventCount()
    {
        var a = new TerrainHeightField(41, 11, 1f);
        var b = new TerrainHeightField(41, 11, 1f);
        var brush = new TerrainBrushSettings(4f, 0.5f, 1f);
        var first = new TerrainSculptStroke(a, brush, false);
        var second = new TerrainSculptStroke(b, brush, false);
        first.MoveTo(new Vector2(5, 5));
        first.MoveTo(new Vector2(35, 5));
        first.Advance(1f);
        for (int x = 5; x <= 35; x++)
            second.MoveTo(new Vector2(x, 5));
        second.Advance(1f);
        Assert.Equal(a.Samples.ToArray(), b.Samples.ToArray());
        for (int x = 5; x <= 35; x++)
            Assert.True(a.GetHeight(x, 5) > 0f);
    }

    [Fact]
    public void MissBreaksPath_AndRepeatedPositionDoesNotStamp()
    {
        var field = new TerrainHeightField(41, 11, 1f);
        var brush = new TerrainBrushSettings(4f, 1f, 1f);
        var stroke = new TerrainSculptStroke(field, brush, true);
        stroke.MoveTo(new Vector2(5, 5));
        Assert.Equal(0, stroke.MoveTo(new Vector2(5, 5)));
        stroke.BreakPath();
        stroke.MoveTo(new Vector2(35, 5));
        Assert.Equal(-1f, field.GetHeight(5, 5));
        Assert.Equal(-1f, field.GetHeight(35, 5));
        Assert.Equal(0f, field.GetHeight(20, 5));
    }
}