using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainLodDefault
{
    [Fact]
    public void LargeTerrain_StartsWithStep20Mesh()
    {
        var field = new TerrainHeightField(301, 301, 2f);
        var map = new MapDocument("LOD 初始级别", field);
        var preview = new TerrainPreviewSession(map);

        var chunk = Assert.Single(preview.Scene.Objects);
        var mesh = chunk.Mesh;
        Assert.NotNull(mesh);
        Assert.Equal(1914, mesh.VertexCount);
    }
}