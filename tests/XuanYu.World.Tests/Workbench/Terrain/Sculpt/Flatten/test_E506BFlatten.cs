using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E506BFlatten
{
    [Fact]
    public void Stamp_CutsAndFillsWithFalloffAndStrength()
    {
        var field = new TerrainHeightField(21, 21, 1f, 1f);
        field.SetHeight(10, 10, 20f);
        field.SetHeight(11, 10, -20f);
        field.SetHeight(18, 10, 99f);
        var brush = new TerrainBrushSettings(8f, 1f, 0.5f);
        Assert.True(TerrainFlattenStamp.Apply(field, new(10, 10), brush, 10f) > 0);
        Assert.Equal(10f, field.GetHeight(10, 10));
        Assert.Equal(10f, field.GetHeight(11, 10));
        Assert.Equal(10f, field.GetHeight(14, 10));
        Assert.Equal(5f, field.GetHeight(16, 10));
        Assert.Equal(99f, field.GetHeight(18, 10));
        var before = field.Samples.ToArray();
        Assert.Equal(0, TerrainFlattenStamp.Apply(field, new(10, 10),
            new TerrainBrushSettings(8f, 0f, 0.5f), -20f));
        Assert.Equal(before, field.Samples.ToArray());
        TerrainFlattenStamp.Apply(field, new(10, 10),
            new TerrainBrushSettings(8f, 0.5f, 0.5f), -10f);
        Assert.Equal(0f, field.GetHeight(10, 10));
    }

    [Fact]
    public void Stroke_LocksTargetAcrossMoveHoldAndBreak()
    {
        var field = new TerrainHeightField(31, 15, 1f, 1f);
        field.SetHeight(5, 7, -10f);
        field.SetHeight(20, 7, 20f);
        var brush = new TerrainBrushSettings(4f, 1f, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, false, flattenHeight: -10f);
        Assert.True(stroke.MoveTo(new(5, 7)) > 0);
        Assert.Equal(-10f, field.GetHeight(6, 7));
        Assert.Equal(0, stroke.MoveTo(new(20, 7)));
        Assert.True(stroke.Advance(0.2f) > 0);
        float first = field.GetHeight(20, 7);
        Assert.InRange(first, -10f, 19.99f);
        Assert.True(stroke.Advance(0.2f) > 0);
        Assert.True(field.GetHeight(20, 7) < first);
        stroke.BreakPath();
        Assert.Equal(0, stroke.Advance(0.1f));
        Assert.True(stroke.MoveTo(new(25, 7)) > 0);
        Assert.Equal(-10f, field.GetHeight(25, 7));
        Assert.Equal(0f, field.GetHeight(20, 0));
    }
}