using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainSeamTransform
{
    [Fact]
    public void AllChunks_UseIdenticalModelToClip()
    {
        var map = new MapDocument("统一矩阵", new TerrainHeightField(601, 601, 2f));
        var session = new TerrainPreviewSession(map);
        var draws = session.Scene.ViewportDraws(800, 600);
        Assert.Equal(4, draws.Count);
        foreach (var draw in draws)
            Assert.Equal(draws[0].ModelToClip, draw.ModelToClip);
    }


    [Fact]
    public void OffsetTile_WaterUsesGlobalSampleCoordinates()
    {
        var field = new TerrainHeightField(601, 301, 2f);
        var samples = new byte[field.SampleCount];
        for (int y = 0; y < 301; y++)
            for (int x = 300; x <= 400; x++)
                samples[y * 601 + x] = 255;
        var water = new TerrainWaterMask(601, 301, samples);
        var mesh = TerrainTileMeshBuilder.Create(field, 300, 0, 50, water);
        ReadOnlySpan<float> values = mesh.Vertices.Span;
        Assert.InRange(values[0], 600f, 800f);
        Assert.True(values[5] > values[4]);
    }
}