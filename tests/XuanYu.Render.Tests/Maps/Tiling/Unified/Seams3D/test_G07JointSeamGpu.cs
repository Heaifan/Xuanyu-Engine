using System.Numerics;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07JointSeamGpu
{
    [G07DataFact]
    public async Task MixedLodCoverageDrawsWithoutHolesAndRestoresPixels()
    {
        var covers = G07SeamCoverage.Build();
        var tiles = covers.SelectMany(c => c.Leaves).Distinct().OrderBy(t => t.Level)
            .ThenBy(t => t.X).ThenBy(t => t.Y).ToArray();
        var keys = tiles.Select((tile, i) => (tile, Key: (long)i + 1))
            .ToDictionary(p => p.tile, p => p.Key);
        var catalog = G07SeamCoverage.Catalog();
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "joint-gpu", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var uploads = new List<TerrainPoolUpload>();
        foreach (var tile in tiles)
        {
            using var lease = await pages.RequestAsync(tile);
            var page = lease.Page;
            var heights = Enumerable.Range(0, 1089)
                .Select(i => page.HeightAt(i % 33, i / 33)!.Value).ToArray();
            uploads.Add(new(keys[tile], heights, page.EncodedNormals.ToArray()));
        }
        var origin = G07SeamCoverage.Origin;
        var mvp = Matrix4x4.CreateScale(0.0004f, 0.0004f, 0.00001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.5f);
        var style = new TerrainVisualStyle(GeoTerrainRenderSession.G07ElevationPalette, true);
        var frames = covers.Select((cover, i) => new TerrainPoolFrame(
            i == 0 ? uploads.ToArray() : [], keys.Values.ToArray(), [], [],
            cover.Leaves.OrderBy(t => keys[t]).Select(t => new TerrainPoolDraw(keys[t], t.South,
                t.West, 1 << t.Level, origin, mvp, cover.EdgeMask(t)) { Style = style }).ToArray())).ToArray();
        using var context = new VkContext();
        var result = context.ProbeTerrainPagePool(16, 16, 16 * 1024 * 1024,
            Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray(), frames);
        Assert.Equal(5, result.Frames.Length);
        foreach (var frame in result.Frames)
        {
            Assert.Equal(128 * 128 * 4, frame.Pixels.Length);
            Assert.Equal(tiles.Length, frame.TotalUploadedPages);
            Assert.Equal(0, frame.PendingForDraw);
            for (int y = 32; y < 96; y++)
                for (int x = 32; x < 96; x++)
                    Assert.True(frame.Pixels[(y * 128 + x) * 4 + 1] > 40,
                        $"混合LOD覆盖内部出现背景或异常暗点：{x},{y}");
        }
        Assert.Equal(result.Frames[0].Pixels, result.Frames[4].Pixels);
        Assert.Equal(result.Frames[1].Pixels, result.Frames[3].Pixels);
    }
}