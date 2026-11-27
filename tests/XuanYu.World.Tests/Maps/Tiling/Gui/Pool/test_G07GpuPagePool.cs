using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07GpuPagePool
{
    [Fact]
    public void BatchReuseAndEvictionPreserveNeighborPixels()
    {
        var first = new TerrainPoolUpload(1, new short[1089]);
        var second = new TerrainPoolUpload(2, Enumerable.Repeat((short)4000, 1089).ToArray());
        var third = new TerrainPoolUpload(3, Enumerable.Repeat((short)2000, 1089).ToArray());
        var left = Draw(1, -0.5f); var right = Draw(2, 0.5f);
        var other = Draw(3, 0.5f);
        TerrainPoolFrame[] frames = [
            new([first, second], [1], [], [2], [left, right]),
            new([first, second], [1, 2], [1], [], [left, right]),
            new([third], [1, 2], [], [], [left, right]),
            new([third], [1], [2], [], [left, other]),
            new([second], [1], [3], [], [left, right]) ];
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        using var context = new VkContext();
        const ulong budget = 16 * 1024 * 1024;
        var result = context.ProbeTerrainPagePool(2, 2, budget, topology, frames);
        Assert.InRange(result.HeightAllocationBytes, 4356UL, budget);
        var f = result.Frames;
        Assert.Equal(new[] { 2, 0, 0, 1, 1 }, f.Select(x => x.SubmittedPages));
        Assert.Equal(new[] { 2, 0, 0, 1, 1 }, f.Select(x => x.PendingBeforePublish));
        Assert.Equal(new[] { 0, 2, 2, 1, 1 }, f.Select(x => x.ResidentBeforePublish));
        Assert.All(f, x => { Assert.Equal(2, x.ResidentForDraw); Assert.Equal(0, x.PendingForDraw); });
        Assert.Equal(new[] { 1, 1, 1, 2, 3 }, f.Select(x => x.TotalBatches));
        Assert.Equal(new[] { 2, 2, 2, 3, 4 }, f.Select(x => x.TotalUploadedPages));
        Assert.False(Assert.Single(f[0].EvictedBeforePublish));
        Assert.False(Assert.Single(f[1].Evicted));
        Assert.True(Assert.Single(f[3].Evicted));
        Assert.True(Assert.Single(f[4].Evicted));
        foreach (var frame in f) Pixel(frame.Pixels, 32, 64, 23, 82, 26);
        Pixel(f[0].Pixels, 96, 64, 179, 179, 161);
        Pixel(f[3].Pixels, 96, 64, 101, 130, 93);
        foreach (int i in new[] { 1, 2, 4 }) Assert.Equal(f[0].Pixels, f[i].Pixels);
        var error = Assert.Throws<InvalidOperationException>(() =>
            context.ProbeTerrainPagePool(2, 2, 1, topology, frames));
        Assert.Contains("显存预算", error.Message);
    }

    private static TerrainPoolDraw Draw(long key, float x)
    {
        var origin = new GeoCoordinate(16d / 3600, 16d / 3600, 0);
        var mvp = Matrix4x4.CreateScale(0.0009f, 0.0015f, 0.0001f) *
            Matrix4x4.CreateTranslation(x, 0, 0.2f);
        return new(key, 324000, 648000, 1, origin, mvp);
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