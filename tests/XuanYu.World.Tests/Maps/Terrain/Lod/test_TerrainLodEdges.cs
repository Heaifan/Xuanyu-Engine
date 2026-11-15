using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainLodEdges
{
    [Theory]
    [InlineData(1, 50)]
    [InlineData(5, 20)]
    [InlineData(20, 50)]
    public void DifferentLods_ShareIdenticalXBorder(int first, int second)
    {
        var field = new TerrainHeightField(601, 301, 2f);
        for (int y = 0; y < field.SamplesY; y++)
            for (int x = 0; x < field.SamplesX; x++)
                field.SetHeight(x, y, 30f * MathF.Sin(y * 0.07f) + x * 0.01f);
        RenderMesh left = TerrainTileMeshBuilder.Create(field, 0, 0,
            first, heightRange: (0f, 200f));
        RenderMesh right = TerrainTileMeshBuilder.Create(field, 300, 0,
            second, heightRange: (0f, 200f));
        var a = Border(left, 600f, vertical: true);
        var b = Border(right, 600f, vertical: true);
        Assert.Equal(63, a.Length);
        Assert.Equal(a, b);
    }

    [Theory]
    [InlineData(1, 20)]
    [InlineData(5, 50)]
    public void DifferentLods_ShareIdenticalYBorder(int first, int second)
    {
        var field = new TerrainHeightField(301, 601, 2f);
        for (int y = 0; y < field.SamplesY; y++)
            for (int x = 0; x < field.SamplesX; x++)
                field.SetHeight(x, y, 30f * MathF.Cos(x * 0.07f) + y * 0.01f);
        RenderMesh below = TerrainTileMeshBuilder.Create(field, 0, 0,
            first, heightRange: (0f, 200f));
        RenderMesh above = TerrainTileMeshBuilder.Create(field, 0, 300,
            second, heightRange: (0f, 200f));
        var a = Border(below, 600f, vertical: false);
        var b = Border(above, 600f, vertical: false);
        Assert.Equal(63, a.Length);
        Assert.Equal(a, b);
    }

    private static (float Z, float R, float G, float B)[] Border(
        RenderMesh mesh, float value, bool vertical)
    {
        var samples = new SortedDictionary<float, (float, float, float, float)>();
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        for (int i = 0; i < data.Length; i += 7)
        {
            float axis = vertical ? data[i] : data[i + 1];
            if (axis != value) continue;
            float along = vertical ? data[i + 1] : data[i];
            samples[along] = (data[i + 2], data[i + 3],
                data[i + 4], data[i + 5]);
        }
        return samples.Values.ToArray();
    }
}