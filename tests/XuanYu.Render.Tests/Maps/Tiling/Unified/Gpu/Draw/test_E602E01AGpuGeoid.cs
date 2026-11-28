using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_E602E01AGpuGeoid
{
    [Fact]
    public async Task RealGpuSessionUploadsGeoidWithoutChangingHeightColor()
    {
        string folder = Path.Combine(Environment.CurrentDirectory, "artifacts", "tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string path = Path.Combine(folder, "N23E120.hgt");
            using (var file = File.Create(path)) file.SetLength(NasademTile.ByteLength);
            var catalog = new NasademTileCatalog([path]);
            var geoid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID") ??
                throw new InvalidOperationException("请设置 XYE_EGM96_GRID。"));
            var tile = new GeoUnifiedTile(0, 33806, 12768);
            var origin = new GeoCoordinate((tile.South + 16) / 3600d - 90d,
                (tile.West + 16) / 3600d - 180d, 0);
            using var context = new VkContext();
            byte[] corrected = await Draw(context, catalog, tile, origin, geoid);
            byte[] legacy = await Draw(context, catalog, tile, origin, null);
            int i = (64 * 128 + 64) * 4;
            Assert.InRange((int)corrected[i], 21, 25);
            Assert.InRange((int)corrected[i + 1], 80, 84);
            Assert.InRange((int)corrected[i + 2], 24, 28);
            Assert.Equal(0, legacy[i + 1]);
        }
        finally { Directory.Delete(folder, true); }
    }

    private static async Task<byte[]> Draw(VkContext context, NasademTileCatalog catalog,
        GeoUnifiedTile tile, GeoCoordinate origin, Egm96GeoidGrid? geoid)
    {
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var samples = new GeoUnifiedSamples(catalog, "e602e01a", loader, geoid);
        var cache = new GeoUnifiedPageCache(loader, samples);
        using var cpu = await GeoUnifiedLodSession.CreateAsync(tile,
            new GeoUnifiedPageLoader(catalog, loader, cache), cache, loader, samples, new EnuFrame(origin));
        using var probe = context.CreateTerrainLodProbe(1, 1, 16 * 1024 * 1024,
            Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray());
        var gpu = new GeoUnifiedGpuSession(cpu, probe.Pool, origin);
        // H=0 的旧几何在近裁剪面之外；真实 N≈21m 将修正几何带入深度范围。
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.01f) *
            Matrix4x4.CreateTranslation(0, 0, -0.1f);
        var eye = new EnuPosition(0, 0, 1000);
        var first = gpu.Step(eye, 128, Math.PI / 3, mvp);
        Assert.Equal(1, first.SubmittedPages);
        probe.Pool.WaitForUpload();
        var frame = gpu.Step(eye, 128, Math.PI / 3, mvp);
        Assert.Single(frame.Draws);
        return probe.Draw(frame.Draws);
    }
}