using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07ViewportClip
{
    [Fact]
    public void FormalRenderPassClipsOutsideImportedCoverage()
    {
        var origin = new GeoCoordinate(16d / 3600, 16d / 3600, 0);
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.0001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.5f);
        var upload = new TerrainPoolUpload(1, new short[1089]);
        var draw = new TerrainPoolDraw(1, 324000, 648000, 1, origin, mvp,
            ClipRects: [new Vector4(0, 0, 16, 32)]);
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        using var context = new VkContext();
        byte[] pixels = context.ProbeTerrainViewport(upload, draw, topology, []);
        int left = (64 * 128 + 32) * 4, right = (64 * 128 + 96) * 4;
        Assert.InRange((int)pixels[left + 1], 80, 84);
        Assert.InRange((int)pixels[right + 1], 25, 29);
    }
}