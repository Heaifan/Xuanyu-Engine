using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainWaterColors
{
    [Fact]
    public void MaskedSea_UsesBlueSurfaceWithoutChangingHeights()
    {
        var field = new TerrainHeightField(301, 301, 2f);
        byte[] samples = new byte[field.SampleCount];
        for (int y = 0; y < 301; y++)
            for (int x = 0; x < 150; x++)
                samples[y * 301 + x] = 255;
        var water = new TerrainWaterMask(301, 301, samples);
        RenderMesh mesh = TerrainTileMeshBuilder.Create(field, 0, 0, 50, water);
        ReadOnlySpan<float> values = mesh.Vertices.Span;
        bool blue = false, green = false;
        for (int i = 0; i < values.Length; i += 7)
        {
            if (values[i] < 200f && values[i + 5] > values[i + 4])
                blue = true;
            if (values[i] > 400f && values[i + 4] > values[i + 5])
                green = true;
            Assert.Equal(0f, values[i + 2]);
        }
        Assert.True(blue);
        Assert.True(green);
    }
}