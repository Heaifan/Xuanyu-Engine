using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainHeightRayPicker
{
    [Fact]
    public void FlatField_UsesOriginalHeight()
    {
        var field = new TerrainHeightField(301, 301, 2f, 3f);
        field.SetHeight(150, 150, 60f);
        bool found = TerrainHeightRayPicker.TryPick(field,
            new Vector3(300f, 450f, 200f), -Vector3.UnitZ, out var hit);
        Assert.True(found);
        Assert.InRange(hit.WorldPosition.Z, 59.999f, 60.001f);
        Assert.Equal(300f, hit.WorldPosition.X);
        Assert.Equal(450f, hit.WorldPosition.Y);
    }

    [Fact]
    public void SlopedField_UsesTriangleInterpolation()
    {
        var field = new TerrainHeightField(2, 2, 2f);
        field.SetHeight(1, 0, 2f);
        field.SetHeight(1, 1, 2f);
        bool found = TerrainHeightRayPicker.TryPick(field,
            new Vector3(0.5f, 0.25f, 5f), -Vector3.UnitZ, out var hit);
        Assert.True(found);
        Assert.InRange(hit.WorldPosition.Z, 0.4999f, 0.5001f);
    }

    [Fact]
    public void OffMapRay_DoesNotHit()
    {
        var field = new TerrainHeightField(2, 2, 2f);
        Assert.False(TerrainHeightRayPicker.TryPick(field,
            new Vector3(4f, 4f, 5f), -Vector3.UnitZ, out _));
    }
}