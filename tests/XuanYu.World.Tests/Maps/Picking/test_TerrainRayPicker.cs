
using System.Numerics;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainRayPicker
{
    [Fact]
    public void FlatTerrain_ReturnsWorldPositionAndDistance()
    {
        var mesh = TerrainMeshBuilder.Build(new TerrainHeightField(2, 2, 2f));

        bool found = TerrainRayPicker.TryPick(mesh,
            new Vector3(0.5f, 0.25f, 5f),
            new Vector3(0f, 0f, -2f), out var hit);

        Assert.True(found);
        Assert.Equal(new Vector3(0.5f, 0.25f, 0f), hit.WorldPosition);
        Assert.Equal(5f, hit.Distance, 4);
        Assert.InRange(hit.TriangleIndex, 0, 1);
    }

    [Fact]
    public void SlopedTerrain_InterpolatesTriangleHeight()
    {
        var field = new TerrainHeightField(2, 2, 2f);
        field.SetHeight(1, 0, 2f);
        field.SetHeight(1, 1, 2f);
        var mesh = TerrainMeshBuilder.Build(field);

        bool found = TerrainRayPicker.TryPick(mesh,
            new Vector3(0.5f, 0.25f, 5f), -Vector3.UnitZ, out var hit);

        Assert.True(found);
        Assert.Equal(0.5f, hit.WorldPosition.Z, 4);
        Assert.Equal(4.5f, hit.Distance, 4);
    }

    [Fact]
    public void OutsideOrParallelRay_DoesNotHit()
    {
        var mesh = TerrainMeshBuilder.Build(new TerrainHeightField(2, 2, 2f));

        Assert.False(TerrainRayPicker.TryPick(mesh,
            new Vector3(3f, 3f, 5f), -Vector3.UnitZ, out _));
        Assert.False(TerrainRayPicker.TryPick(mesh,
            new Vector3(1f, 1f, 5f), Vector3.UnitX, out _));
    }

    [Fact]
    public void OverlappingTriangles_SelectsNearestIntersection()
    {
        Vector3[] positions =
        [
            new(0, 0, 0), new(2, 0, 0), new(0, 2, 0),
            new(0, 0, 2), new(2, 0, 2), new(0, 2, 2)
        ];
        Vector3[] normals =
        [
            Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ,
            Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ
        ];
        var mesh = new TerrainMeshGeometry(positions, normals,
            [0, 1, 2, 3, 4, 5]);

        Assert.True(TerrainRayPicker.TryPick(mesh,
            new Vector3(0.25f, 0.25f, 5f), -Vector3.UnitZ, out var hit));
        Assert.Equal(2f, hit.WorldPosition.Z, 4);
        Assert.Equal(3f, hit.Distance, 4);
        Assert.Equal(1, hit.TriangleIndex);
    }

    [Fact]
    public void ReverseDirection_HitsTerrainFromBelow()
    {
        var mesh = TerrainMeshBuilder.Build(new TerrainHeightField(2, 2, 2f));

        Assert.True(TerrainRayPicker.TryPick(mesh,
            new Vector3(0.5f, 0.25f, -2f), Vector3.UnitZ, out var hit));
        Assert.Equal(0f, hit.WorldPosition.Z);
        Assert.Equal(2f, hit.Distance, 4);
    }
}
