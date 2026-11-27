using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M01BMountainShape
{
    [Fact]
    public void BreakPath_PendingSegmentsKeepTheirOriginalShape()
    {
        var early = new TerrainHeightField(81, 41, 1f);
        var late = new TerrainHeightField(81, 41, 1f);
        var brush = new TerrainBrushSettings(8f, 1f, 0.5f);
        var a = new TerrainSculptStroke(early, brush, false, naturalPeakHeight: 30f);
        var b = new TerrainSculptStroke(late, brush, false, naturalPeakHeight: 30f);
        a.MoveTo(new(10, 10)); a.MoveTo(new(30, 10)); a.Advance(0f);
        a.BreakPath(); a.MoveTo(new(60, 30));
        b.MoveTo(new(10, 10)); b.MoveTo(new(30, 10));
        b.BreakPath(); b.MoveTo(new(60, 30)); b.Advance(0f);
        Assert.Equal(early.Samples.ToArray(), late.Samples.ToArray());
        Assert.InRange(late.GetHeight(60, 30), 0.001f, 30f);
        Assert.Equal(0f, late.GetHeight(45, 20));
    }
}