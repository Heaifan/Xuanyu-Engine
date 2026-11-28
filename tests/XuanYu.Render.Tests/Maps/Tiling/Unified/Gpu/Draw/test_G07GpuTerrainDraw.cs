using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07GpuTerrainDraw
{
    [Fact]
    public void AllEdgeMasksDrawHeightColorsAndReuseTheSamePage()
    {
        const int south = 324000, west = 648000;
        var heights = new short[1089];
        for (int y = 0; y < 33; y++)
            for (int x = 17; x < 33; x++) heights[y * 33 + x] = 4000;
        var origin = new GeoCoordinate(16d / 3600, 16d / 3600, 0);
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.0001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.2f);
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        int[] masks = [.. Enumerable.Range(0, 16), 0];
        using var context = new VkContext();
        var frames = context.ProbeTerrainDraws(south, west, 1, origin, heights, mvp, topology, masks);
        Assert.Equal(17, frames.Length);
        foreach (var frame in frames)
        {
            Assert.Equal(128 * 128 * 4, frame.Length);
            Pixel(frame, 4, 4, 0, 0, 0);
            Pixel(frame, 32, 64, 23, 82, 26);
            Pixel(frame, 96, 64, 179, 179, 161);
            for (int y = 24; y < 104; y++)
                for (int x = 24; x < 104; x++)
                    Assert.True(frame[(y * 128 + x) * 4 + 1] >= 75, $"地形内部出现空洞：{x},{y}");
        }
        Assert.True(frames[0].AsSpan().SequenceEqual(frames[^1]), "相同掩码重复绘制结果不一致。");
    }

    [G07DataFact]
    public async Task RealNasademPageDrawsWithoutCpuVertexMesh()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var tile = new GeoUnifiedTile(0, 33861, 12736);
        using var lease = await new GeoUnifiedPageLoader(catalog, loader, cache).RequestAsync(tile);
        var heights = new short[1089];
        for (int y = 0; y < 33; y++)
            for (int x = 0; x < 33; x++) heights[y * 33 + x] = lease.Page.HeightAt(x, y)!.Value;
        var origin = new GeoCoordinate((tile.South + 16) / 3600d - 90,
            (tile.West + 16) / 3600d - 180, 0);
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.00001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.5f);
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        using var context = new VkContext();
        var frames = context.ProbeTerrainDraws(tile.South, tile.West, 1, origin, heights, mvp, topology, [0, 15, 0]);
        foreach (var frame in frames)
        {
            Pixel(frame, 4, 4, 0, 0, 0);
            for (int y = 24; y < 104; y++)
                for (int x = 24; x < 104; x++)
                    Assert.True(frame[(y * 128 + x) * 4 + 1] >= 75, $"真实地形内部出现空洞：{x},{y}");
        }
        Assert.Equal(frames[0], frames[^1]);
    }

    private static void Pixel(byte[] frame, int x, int y, int r, int g, int b)
    {
        int i = (y * 128 + x) * 4;
        Assert.InRange((int)frame[i], r - 2, r + 2);
        Assert.InRange((int)frame[i + 1], g - 2, g + 2);
        Assert.InRange((int)frame[i + 2], b - 2, b + 2);
        Assert.Equal((byte)255, frame[i + 3]);
    }
}