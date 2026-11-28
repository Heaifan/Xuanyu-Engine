using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07ViewportPipeline
{
    [Fact]
    public void FormalRenderPassSharesDepthBetweenTerrainAndSceneMeshes()
    {
        var origin = new GeoCoordinate(16d / 3600, 16d / 3600, 0);
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.0001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.5f);
        var upload = new TerrainPoolUpload(1, new short[1089]);
        var draw = new TerrainPoolDraw(1, 324000, 648000, 1, origin, mvp);
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        using var context = new VkContext();
        byte[] pixels = context.ProbeTerrainViewport(upload, draw, topology,
            [Quad("behind", -0.8f, -0.2f, 0.8f), Quad("front", 0.2f, 0.8f, 0.1f)]);
        Assert.Equal(128 * 128 * 4, pixels.Length);
        Pixel(pixels, 4, 4, 9, 27, 56);
        Pixel(pixels, 32, 64, 23, 82, 26);
        Pixel(pixels, 96, 64, 255, 0, 0);
    }

    private static RenderDraw Quad(string id, float left, float right, float depth)
    {
        float[] vertices = [
            left, -0.5f, depth, 1f, 0f, 0f, 1f,
            right, -0.5f, depth, 1f, 0f, 0f, 1f,
            right, 0.5f, depth, 1f, 0f, 0f, 1f,
            left, -0.5f, depth, 1f, 0f, 0f, 1f,
            right, 0.5f, depth, 1f, 0f, 0f, 1f,
            left, 0.5f, depth, 1f, 0f, 0f, 1f
        ];
        return new RenderDraw(new RenderMesh(id, vertices), Matrix4x4.Identity, true, false);
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