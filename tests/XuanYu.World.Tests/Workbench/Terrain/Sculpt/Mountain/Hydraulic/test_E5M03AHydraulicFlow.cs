using Xunit;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5M03AHydraulicFlow
{
    [Fact]
    public void Plane_UsesPhysicalCellAreaAndCollectsDownstream()
    {
        var heights = new float[7 * 5];
        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 7; x++) heights[y * 7 + x] = 6 - x;
        var result = TerrainHydraulicErosion.Apply(heights, 7, 2f, 3f, 1);
        Assert.Equal(1, result.Iterations);
        Assert.Equal(42d, result.MaxDrainageAreaSquareMeters);
        Assert.True(result.MaxCutMeters > 0f);
        Assert.True(result.MaxFillMeters > 0f);
    }

    [Fact]
    public void Valley_CollectsBothSlopesAtItsOutlet()
    {
        var heights = new float[5 * 5];
        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
            heights[y * 5 + x] = 2f * (4 - y) + 5f * Math.Abs(x - 2);
        var result = TerrainHydraulicErosion.Apply(heights, 5, 1f, 1f, 1);
        Assert.Equal(25d, result.MaxDrainageAreaSquareMeters);
        Assert.True(heights[22] > 0f);
        Assert.True(result.MaxCutMeters > 0f);
    }

    [Fact]
    public void FlatAndZeroPasses_LeaveInputUntouched()
    {
        var flat = Enumerable.Repeat(10f, 35).ToArray();
        var before = (float[])flat.Clone();
        var result = TerrainHydraulicErosion.Apply(flat, 7, 2f, 3f, 16);
        Assert.Equal(1, result.Iterations);
        Assert.Equal(6d, result.MaxDrainageAreaSquareMeters);
        Assert.Equal(0f, result.MaxCutMeters);
        Assert.Equal(0f, result.MaxFillMeters);
        Assert.Equal(before, flat);
        flat[17] = 30f;
        before = (float[])flat.Clone();
        result = TerrainHydraulicErosion.Apply(flat, 7, 2f, 3f, 0);
        Assert.Equal(0, result.Iterations);
        Assert.Equal(before, flat);
    }

    [Fact]
    public void InvalidReliefAndDimensions_AreRejectedAtEntry()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TerrainHydraulicErosion.Apply(new float[5], 2, 1f, 1f, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TerrainHydraulicErosion.Apply(new[] { 0f, -1f, 0f, 0f }, 2, 1f, 1f, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TerrainHydraulicErosion.Apply(new float[4], 2, 0f, 1f, 1));
    }
}