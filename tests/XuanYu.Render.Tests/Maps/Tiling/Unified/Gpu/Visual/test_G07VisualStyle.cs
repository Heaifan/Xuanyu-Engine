using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07VisualStyle
{
    [Fact]
    public void PaletteHillshadeAndWaterUseTheProductionPoolShader()
    {
        const int side = 3601, header = 3;
        int packedSide = (int)Math.Ceiling(Math.Sqrt(header + (side * side + 15) / 16));
        var packed = new short[packedSide * packedSide];
        packed[0] = packed[1] = packed[2] = 1;
        for (int y = 0; y < 40; y++)
            for (int x = 16; x < 40; x++)
            {
                int index = y * side + x;
                packed[header + index / 16] |= unchecked((short)(1 << (index % 16)));
            }
        var water = new RenderWaterAtlas(0, 0, packedSide, packed, 1);
        var palette = GeoTerrainRenderSession.G07ElevationPalette;
        var origin = new GeoCoordinate(16d / 3600, 16d / 3600, 0);
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.0001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.2f);
        var draw = new TerrainPoolDraw(1, 324000, 648000, 1, origin, mvp);
        var flat = draw with { Style = new TerrainVisualStyle(palette, false) };
        var shaded = draw with { Style = new TerrainVisualStyle(palette, true) };
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        TerrainPoolFrame[] frames = [new([new(1, new short[1089])], [1], [], [], [flat]),
            new([], [1], [], [], [shaded]), new([], [1], [], [], [flat])];
        using var context = new VkContext();
        var result = context.ProbeTerrainPagePool(2, 1, 16 * 1024 * 1024, topology, frames, water);
        Pixel(result.Frames[0].Pixels, 32, 64, 92, 187, 111);
        Pixel(result.Frames[1].Pixels, 32, 64, 85, 173, 103);
        Pixel(result.Frames[0].Pixels, 96, 64, 82, 156, 214);
        Pixel(result.Frames[1].Pixels, 96, 64, 82, 156, 214);
        Assert.Equal(result.Frames[0].Pixels, result.Frames[2].Pixels);
        Assert.All(result.Frames, frame => Assert.Equal(1, frame.TotalUploadedPages));
    }

    private static void Pixel(byte[] pixels, int x, int y, int r, int g, int b)
    {
        int i = (y * 128 + x) * 4;
        Assert.InRange((int)pixels[i], r - 2, r + 2);
        Assert.InRange((int)pixels[i + 1], g - 2, g + 2);
        Assert.InRange((int)pixels[i + 2], b - 2, b + 2);
        Assert.Equal((byte)255, pixels[i + 3]);
    }
}