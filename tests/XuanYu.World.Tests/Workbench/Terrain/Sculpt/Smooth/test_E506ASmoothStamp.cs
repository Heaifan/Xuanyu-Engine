using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E506ASmoothStamp
{
    [Fact]
    public void PeakAndPit_SmoothSymmetricallyWithoutChangingOutside()
    {
        foreach (float sign in new[] { 1f, -1f })
        {
            var field = new TerrainHeightField(9, 9, 1f, 1f);
            field.SetHeight(4, 4, 16f * sign);
            field.SetHeight(0, 0, 99f);
            var brush = new TerrainBrushSettings(3f, 1f, 0f);
            Assert.Equal(5, TerrainSmoothStamp.Apply(field, new(4, 4), brush));
            Assert.Equal(8f * sign, field.GetHeight(4, 4));
            Assert.Equal(2f * sign, field.GetHeight(3, 4));
            Assert.Equal(2f * sign, field.GetHeight(5, 4));
            Assert.Equal(2f * sign, field.GetHeight(4, 3));
            Assert.Equal(2f * sign, field.GetHeight(4, 5));
            Assert.Equal(0f, field.GetHeight(3, 3));
            Assert.Equal(99f, field.GetHeight(0, 0));
        }
    }

    [Fact]
    public void FlatAndInteriorPlane_RemainUnchanged()
    {
        var field = new TerrainHeightField(9, 9, 2f, 3f);
        for (int y = 0; y < 9; y++)
        for (int x = 0; x < 9; x++) field.SetHeight(x, y, -20f);
        var brush = new TerrainBrushSettings(100f, 1f, 0.5f);
        Assert.Equal(0, TerrainSmoothStamp.Apply(field, new(0, 0), brush));
        for (int y = 0; y < 9; y++)
        for (int x = 0; x < 9; x++) field.SetHeight(x, y, x * 2f + y * 3f);
        Assert.Equal(0, TerrainSmoothStamp.Apply(field, new(8, 12),
            new TerrainBrushSettings(4f, 1f, 0.5f)));
    }

    [Fact]
    public void ZeroStrengthAndFalloff_RespectBrushSettings()
    {
        var field = new TerrainHeightField(9, 9, 1f, 1f);
        field.SetHeight(4, 4, 16f);
        var before = field.Samples.ToArray();
        Assert.Equal(0, TerrainSmoothStamp.Apply(field, new(4, 4),
            new TerrainBrushSettings(2f, 0f, 1f)));
        Assert.Equal(before, field.Samples.ToArray());
        TerrainSmoothStamp.Apply(field, new(4, 4), new TerrainBrushSettings(2f, 1f, 1f));
        Assert.Equal(8f, field.GetHeight(4, 4));
        Assert.Equal(1f, field.GetHeight(5, 4));
        Assert.Equal(0f, field.GetHeight(6, 4));
    }
}