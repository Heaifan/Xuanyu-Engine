using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07GpuRealPagePool
{
    [G07DataFact]
    public async Task AdjacentRealPagesRespectBatchLimitAndShareOneDrawPass()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var cache = new GeoUnifiedPageCache(loader, new GeoUnifiedSamples(catalog, "v1", loader));
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var first = new GeoUnifiedTile(0, 33861, 12736);
        var second = new GeoUnifiedTile(0, 33862, 12736);
        using var a = await pages.RequestAsync(first);
        using var b = await pages.RequestAsync(second);
        var origin = new GeoCoordinate((first.South + 16) / 3600d - 90,
            (first.West + 32) / 3600d - 180, 0);
        var mvp = Matrix4x4.CreateScale(0.0009f, 0.0015f, 0.00001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.5f);
        TerrainPoolUpload[] uploads = [new(1, Heights(a.Page)), new(2, Heights(b.Page))];
        var left = new TerrainPoolDraw(1, first.South, first.West, 1, origin, mvp);
        var right = new TerrainPoolDraw(2, second.South, second.West, 1, origin, mvp);
        TerrainPoolFrame[] frames = [new(uploads, [1], [], [], [left]),
            new(uploads, [1, 2], [], [], [left], PublishBeforeDraw: false),
            new(uploads, [1, 2], [], [], [left, right]),
            new(uploads, [1, 2], [], [], [left, right])];
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        using var context = new VkContext();
        var f = context.ProbeTerrainPagePool(2, 1, 16 * 1024 * 1024, topology, frames).Frames;
        Assert.Equal(new[] { 1, 1, 0, 0 }, f.Select(x => x.SubmittedPages));
        Assert.Equal(new[] { 1, 1, 2, 2 }, f.Select(x => x.ResidentForDraw));
        Assert.Equal(new[] { 1, 2, 2, 2 }, f.Select(x => x.TotalBatches));
        Assert.Equal(new[] { 0, 1, 0, 0 }, f.Select(x => x.PendingForDraw));
        Assert.Equal(2, f[3].TotalUploadedPages);
        Assert.Equal(f[0].Pixels, f[1].Pixels);
        Assert.Equal((byte)0, f[0].Pixels[(64 * 128 + 96) * 4 + 1]);
        for (int y = 32; y < 96; y++)
            for (int x = 24; x < 104; x++)
                Assert.True(f[2].Pixels[(y * 128 + x) * 4 + 1] >= 75, $"相邻页内部空洞：{x},{y}");
        Assert.Equal(f[2].Pixels, f[3].Pixels);
    }

    private static short[] Heights(GeoUnifiedHeightPage page)
    {
        var values = new short[1089];
        for (int y = 0; y < 33; y++)
            for (int x = 0; x < 33; x++) values[y * 33 + x] = page.HeightAt(x, y)!.Value;
        return values;
    }
}