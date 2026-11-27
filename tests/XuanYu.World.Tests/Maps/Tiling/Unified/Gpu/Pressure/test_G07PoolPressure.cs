using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07PoolPressure
{
    [Fact]
    public void RealPoolKeepsProtectedPagesWhileFilling512Slots()
    {
        var heights = new short[33 * 33];
        var frames = new List<TerrainPoolFrame>();
        for (int first = 1; first <= 497; first += 16)
        {
            int count = Math.Min(16, 508 - first + 1);
            var uploads = Enumerable.Range(first, count)
                .Select(key => new TerrainPoolUpload(key, heights)).ToArray();
            frames.Add(new(uploads, Enumerable.Range(1, 512).Select(i => (long)i).ToArray(),
                first == 497 ? [1] : [], [], []));
        }
        frames.Add(new(Enumerable.Range(509, 4).Select(key => new TerrainPoolUpload(key, heights)).ToArray(),
            Enumerable.Range(1, 512).Select(i => (long)i).ToArray(), [], [509], []));
        using var context = new VkContext();
        var result = context.ProbeTerrainPagePool(512, 16, 16 * 1024 * 1024,
            Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray(), frames);
        Assert.Equal(508, result.Frames[^2].ResidentForDraw);
        Assert.False(Assert.Single(result.Frames[^2].Evicted));
        Assert.Equal(512, result.Frames[^1].ResidentForDraw);
        Assert.Equal(4, result.Frames[^1].PendingBeforePublish);
        Assert.False(Assert.Single(result.Frames[^1].EvictedBeforePublish));
        Assert.Equal(512, result.Frames[^1].TotalUploadedPages);
    }
}
