using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07GpuLodFlow
{
    [G07DataFact]
    public Task CrossHgtRefineCoarsenAndRefineWithoutHoles() => Run(5);

    [G07DataFact]
    public Task InsufficientCapacityKeepsCommittedRootWithoutPartialUploads() => Run(4);

    private static async Task Run(int capacity)
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var root = new GeoUnifiedTile(1, 16931, 6368);
        var origin = new GeoCoordinate((root.South + 32) / 3600d - 90,
            (root.West + 32) / 3600d - 180, 0);
        var anchor = new EnuFrame(origin);
        using var cpu = await GeoUnifiedLodSession.CreateAsync(root, pages, cache, loader, source, anchor);
        using var context = new VkContext();
        using var probe = context.CreateTerrainLodProbe(capacity, 1, 16 * 1024 * 1024,
            Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray());
        var session = new GeoUnifiedGpuSession(cpu, probe.Pool, origin);
        var mvp = Matrix4x4.CreateScale(0.00065f, 0.00065f, 0.00001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.5f);
        EnuPosition near;
        using (var lease = cpu.AcquirePage(root, 0)!)
            near = GeoUnifiedPosition.At(anchor, root.West + 32, root.South + 32,
                lease.Page.HeightAt(16, 16)!.Value);
        var far = new EnuPosition(0, 0, 100_000_000);
        var drive = new G07GpuLodDriver(session, probe, mvp, root);
        await drive.Until(near, frame => frame.Draws.Length == 1, 0);
        if (capacity == 4)
        {
            await drive.Until(near, frame => frame.CapacityBlocked, 1);
            for (int i = 0; i < 4; i++)
            {
                var frame = drive.Tick(near);
                Assert.True(frame.CapacityBlocked);
                Assert.Single(frame.Draws);
            }
            Assert.Equal(1, probe.Pool.UploadedPages);
            Assert.Equal(1, probe.Pool.ResidentCount);
            return;
        }
        await drive.Until(near, frame => frame.Draws.Length == 4, 1);
        Assert.Equal(5, probe.Pool.UploadedPages);
        Assert.Equal(4, probe.Pool.ResidentCount);
        await drive.Until(far, frame => frame.Draws.Length == 1, 4);
        Assert.Equal(6, probe.Pool.UploadedPages);
        Assert.Equal(1, probe.Pool.ResidentCount);
        await drive.Until(near, frame => frame.Draws.Length == 4, 1);
        Assert.Equal(10, probe.Pool.UploadedPages);
        Assert.Equal(4, probe.Pool.ResidentCount);
        var stable = drive.Tick(near);
        Assert.Equal(4, stable.Draws.Length);
        Assert.Equal(0, stable.SubmittedPages);
        Assert.Equal(10, probe.Pool.UploadedPages);
    }
}