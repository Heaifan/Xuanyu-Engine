using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainLodTopology
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(20)]
    [InlineData(50)]
    public void AllLods_CoverTileWithoutReversedOrDegenerateFaces(int step)
    {
        var field = new TerrainHeightField(301, 301, 2f);
        RenderMesh mesh = TerrainTileMeshBuilder.Create(field, 0, 0, step);
        ReadOnlySpan<float> v = mesh.Vertices.Span;
        double projectedArea = 0;
        for (int i = 0; i < v.Length; i += 21)
        {
            double ax = v[i], ay = v[i + 1];
            double bx = v[i + 7], by = v[i + 8];
            double cx = v[i + 14], cy = v[i + 15];
            double cross = (bx - ax) * (cy - ay) -
                (cx - ax) * (by - ay);
            Assert.True(cross > 0, $"step={step} 存在退化或翻面三角形");
            projectedArea += cross * 0.5;
        }
        Assert.Equal(360000d, projectedArea, 3);
    }
}