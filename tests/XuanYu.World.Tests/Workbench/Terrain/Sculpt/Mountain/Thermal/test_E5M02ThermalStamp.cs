using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M02ThermalStamp
{
    [Fact]
    public void Peak_TransfersToNeighborsAndConservesHeight()
    {
        var field = new TerrainHeightField(21, 21, 1f);
        field.SetHeight(10, 10, 20f);
        field.SetHeight(20, 20, 99f);
        var brush = new TerrainBrushSettings(6f, 1f, 0.5f);
        double before = field.Samples.ToArray().Sum(h => (double)h);
        Assert.True(TerrainThermalStamp.Apply(field, new(10, 10), brush) > 0);
        Assert.InRange(field.GetHeight(10, 10), 10f, 19.99f);
        Assert.True(field.GetHeight(11, 10) > 0f);
        Assert.True(field.GetHeight(11, 11) > 0f);
        Assert.Equal(99f, field.GetHeight(20, 20));
        for (int i = 0; i < 20; i++) TerrainThermalStamp.Apply(field, new(10, 10), brush);
        Assert.InRange(Math.Abs(field.Samples.ToArray().Sum(h => (double)h) - before), 0d, 0.001d);
        Assert.Equal(0f, field.GetHeight(16, 10));
    }

    [Fact]
    public void ShallowPlaneAndZeroStrength_DoNotChangeField()
    {
        var field = new TerrainHeightField(11, 11, 2f, 4f);
        for (int y = 0; y < field.SamplesY; y++)
        for (int x = 0; x < field.SamplesX; x++) field.SetHeight(x, y, x + y - 10f);
        var before = field.Samples.ToArray();
        Assert.Equal(0, TerrainThermalStamp.Apply(field, new(10, 20),
            new TerrainBrushSettings(100f, 1f, 0f)));
        Assert.Equal(before, field.Samples.ToArray());
        field.SetHeight(5, 5, 100f);
        before = field.Samples.ToArray();
        Assert.Equal(0, TerrainThermalStamp.Apply(field, new(10, 20),
            new TerrainBrushSettings(100f, 0f, 1f)));
        Assert.Equal(before, field.Samples.ToArray());
    }

    [Fact]
    public void NonSquareSpacing_UsesPhysicalSlope()
    {
        var field = new TerrainHeightField(5, 5, 1f, 4f);
        field.SetHeight(2, 2, 2f);
        TerrainThermalStamp.Apply(field, new(2, 8), new TerrainBrushSettings(20f, 1f, 0f));
        Assert.True(field.GetHeight(3, 2) > 0f);
        Assert.Equal(0f, field.GetHeight(2, 3));
        Assert.Equal(0f, field.GetHeight(3, 3));
    }
}