using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07NormalGpu
{
    [Fact]
    public async Task ProductionSessionUploadsGeographicNormalsAndShaderUsesThem()
    {
        using var fixture = new G07NormalFixture();
        var root = G07NormalFixture.Right;
        var origin = new GeoCoordinate((root.South + 16) / 3600d - 90,
            (root.West + 16) / 3600d - 180, 0);
        var loader = new GeoUnifiedTileLoader(fixture.Catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(fixture.Catalog, "normal-gpu", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(fixture.Catalog, loader, cache);
        using var cpu = await GeoUnifiedLodSession.CreateAsync(root, pages, cache, loader, source, new EnuFrame(origin));
        using var context = new VkContext();
        using var probe = context.CreateTerrainLodProbe(2, 1, 16 * 1024 * 1024,
            Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray());
        var session = new GeoUnifiedGpuSession(cpu, probe.Pool, origin);
        var mvp = Matrix4x4.CreateScale(0.001f, 0.001f, 0.00001f) * Matrix4x4.CreateTranslation(0, 0, 0.5f);
        var eye = new EnuPosition(0, 0, 20000);
        session.Step(eye, 128, Math.PI / 3, mvp);
        probe.Pool.WaitForUpload();
        var frame = session.Step(eye, 128, Math.PI / 3, mvp);
        var draw = Assert.Single(frame.Draws);
        var palette = GeoTerrainRenderSession.G07ElevationPalette;
        var plain = probe.Draw([draw with { Style = new TerrainVisualStyle(palette, false) }]);
        var shade = probe.Draw([draw with { Style = new TerrainVisualStyle(palette, true) }]);
        int i = (64 * 128 + 64) * 4;
        Assert.True(plain[i + 1] > 80, "中心必须命中实际地形而非背景。");
        Assert.InRange((int)shade[i + 1], (int)(plain[i + 1] * 0.75f) - 3,
            (int)(plain[i + 1] * 0.75f) + 3);
        Assert.Equal((byte)255, shade[i + 3]);
        Assert.Equal(1, probe.Pool.UploadedPages);
    }
}