
using System.Numerics;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestMapDocument
{
    [Fact]
    public void DefaultMap_HasCorrectDimensions()
    {
        MapDocument map = MapDocument.CreateDefault("测试地图");

        Assert.Equal(1, map.Version);
        Assert.Equal(256f, map.WidthMeters);
        Assert.Equal(256f, map.LengthMeters);
        Assert.Equal(129 * 129, map.HeightField.SampleCount);
    }

    [Fact]
    public void HeightField_UsesXYGroundAndZHeight()
    {
        var field = new TerrainHeightField(129, 129, 2f);
        field.SetHeight(64, 12, 32.5f);

        Assert.Equal(32.5f, field.GetHeight(64, 12));
        Assert.Equal(
            new Vector3(128f, 24f, 32.5f),
            field.GetWorldPosition(64, 12));
    }

    [Fact]
    public void ChangingHeight_DoesNotChangeGridDimensions()
    {
        var field = new TerrainHeightField(3, 3, 2f);
        field.SetHeight(1, 1, 25f);
        field.SetHeight(2, 1, -5f);

        Assert.Equal(4f, field.WidthMeters);
        Assert.Equal(4f, field.LengthMeters);
        Assert.Equal(25f, field.GetHeight(1, 1));
        Assert.Equal(-5f, field.GetHeight(2, 1));
        Assert.Equal(0f, field.GetHeight(0, 0));
    }

    [Fact]
    public void InvalidMapInputs_AreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TerrainHeightField(1, 3, 2f));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TerrainHeightField(3, 3, 0f));

        var field = new TerrainHeightField(3, 3, 2f);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => field.SetHeight(1, 1, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => field.GetHeight(3, 0));
    }
}
