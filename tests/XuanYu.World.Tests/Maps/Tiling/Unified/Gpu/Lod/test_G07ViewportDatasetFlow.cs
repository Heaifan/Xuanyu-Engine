using System.Diagnostics;
using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07ViewportDatasetFlow
{
    [G07DataFact]
    public async Task MultiRootGpuCoverageRefinesWithoutLosingImportedArea()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var origin = new GeoCoordinate(23.5, 121, 0);
        using var cpu = await GeoUnifiedLodSession.CreateDatasetAsync(catalog, new EnuFrame(origin), 128);
        int roots = cpu.Coverage.Leaves.Count;
        using var context = new VkContext();
        using var probe = context.CreateTerrainLodProbe(128, 4, 16 * 1024 * 1024,
            Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray());
        var gpu = new GeoUnifiedGpuSession(cpu, probe.Pool, origin);
        var mvp = Matrix4x4.CreateScale(0.000008f, 0.000008f, 0.00001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.5f);
        var watch = Stopwatch.StartNew();
        bool initial = false;
        while (watch.Elapsed < TimeSpan.FromSeconds(90))
        {
            var frame = gpu.Step(new EnuPosition(0, 0, 2000), 1080, Math.PI / 3, mvp);
            Assert.InRange(frame.SubmittedPages, 0, 4);
            Assert.InRange(probe.Pool.ResidentCount + probe.Pool.PendingCount, 0, 128);
            if (frame.Draws.Length != 0)
            {
                Assert.Equal(frame.Coverage.Leaves.Count, frame.Draws.Length);
                Assert.All(frame.Draws, d => Assert.True(probe.Pool.IsReady(d.Key)));
                double area = frame.Draws.Sum(d => d.ClipRects!.Sum(r =>
                    (double)(r.Z - r.X) * (r.W - r.Y) * d.Step * d.Step));
                Assert.Equal(2d * 3600 * 3600, area);
                if (!initial)
                {
                    Assert.Equal(roots, frame.Draws.Length);
                    Assert.Equal(128 * 128 * 4, probe.Draw(frame.Draws).Length);
                    initial = true;
                }
                else if (frame.Draws.Length > roots)
                {
                    Assert.Equal(128 * 128 * 4, probe.Draw(frame.Draws).Length);
                    return;
                }
            }
            await Task.Delay(10);
        }
        Assert.Fail($"G07 多根覆盖未完成细化：首屏={initial} 指标={cpu.Diagnostic}");
    }
}