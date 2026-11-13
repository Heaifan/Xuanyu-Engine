
using System.Numerics;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainMeshBuilder
{
    [Fact]
    public void DefaultMap_GeneratesExpectedGeometry()
    {
        var map = MapDocument.CreateDefault("测试地图");
        var mesh = TerrainMeshBuilder.Build(map.HeightField);

        Assert.Equal(16641, mesh.VertexCount);
        Assert.Equal(32768, mesh.TriangleCount);
        Assert.Equal(98304, mesh.Indices.Length);
        Assert.Equal(new Vector3(256f, 256f, 0f),
            mesh.Positions[^1]);
    }

    [Fact]
    public void FlatTerrain_HasCorrectWindingAndNormals()
    {
        var field = new TerrainHeightField(2, 2, 2f);
        var mesh = TerrainMeshBuilder.Build(field);

        Assert.Equal(new[] { 0, 1, 3, 0, 3, 2 },
            mesh.Indices);

        Assert.All(mesh.Normals,
            normal => Assert.Equal(Vector3.UnitZ, normal));
    }

    [Fact]
    public void Slope_HasUpwardNormalizedNormals()
    {
        var field = new TerrainHeightField(3, 3, 2f);

        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
                field.SetHeight(x, y, x * 2f);

        var mesh = TerrainMeshBuilder.Build(field);
        Vector3 expected = Vector3.Normalize(new Vector3(-1, 0, 1));

        Assert.All(mesh.Normals, normal =>
        {
            Assert.True(Vector3.Dot(normal, expected) > 0.999f);
            Assert.InRange(normal.Length(), 0.999f, 1.001f);
        });
    }

    [Fact]
    public void BuildingMesh_DoesNotModifySourceHeights()
    {
        var field = new TerrainHeightField(2, 2, 2f);
        field.SetHeight(0, 0, 6f);

        var mesh = TerrainMeshBuilder.Build(field);
        field.SetHeight(0, 0, 20f);

        Assert.Equal(6f, mesh.Positions[0].Z);
        Assert.Equal(20f, field.GetHeight(0, 0));
    }
}
