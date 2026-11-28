using System.Diagnostics;
using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07SessionPressure
{
    [G07DataFact]
    public async Task MultiRootNearCapacityCanMoveBetweenRegionsAndBack()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var origin = new GeoCoordinate(23.5, 121, 0);
        var anchor = new EnuFrame(origin);
        int roots = new GeoUnifiedViewportDomain(catalog.Coordinates).Roots.Count;
        int capacity = roots + 4;
        using var cpu = await GeoUnifiedLodSession.CreateDatasetAsync(catalog, anchor, capacity);
        using var context = new VkContext();
        using var probe = context.CreateTerrainLodProbe(capacity, 4, 16 * 1024 * 1024,
            Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray());
        var gpu = new GeoUnifiedGpuSession(cpu, probe.Pool, origin);
        var mvp = Matrix4x4.CreateScale(0.000008f, 0.000008f, 0.00001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.5f);
        var a = new GeoUnifiedTile(6, 528, 199);
        var b = new GeoUnifiedTile(6, 529, 199);
        Assert.Contains(a, cpu.Coverage.Leaves);
        Assert.Contains(b, cpu.Coverage.Leaves);
        bool initialized = false;
        foreach (var region in new[] { a, b, a })
        {
            var eye = GeoUnifiedPosition.At(anchor, region.West + region.Span / 2,
                region.South + region.Span / 2, 2000);
            var clock = Stopwatch.StartNew();
            bool reached = false;
            while (clock.Elapsed < TimeSpan.FromSeconds(90))
            {
                // Controlled visibility stresses the real CPU/GPU session with one transition slot left.
                var frame = gpu.Step(eye, 100000, Math.PI / 3, mvp,
                    tile => tile.West >= region.West && tile.South >= region.South &&
                        tile.West + tile.Span <= region.West + region.Span &&
                        tile.South + tile.Span <= region.South + region.Span);
                Assert.InRange(probe.Pool.ResidentCount + probe.Pool.PendingCount, 0, capacity);
                if (frame.PendingCoverage is { } pending)
                    Assert.InRange(frame.Coverage.Leaves.Union(pending.Leaves).Count(), 1, capacity);
                if (frame.CapacityBlocked) Assert.Null(frame.PendingCoverage);
                if (frame.Draws.Length != 0)
                {
                    initialized = true;
                    Assert.Equal(frame.Coverage.Leaves.Count, frame.Draws.Length);
                    Assert.All(frame.Draws, d => Assert.True(probe.Pool.IsReady(d.Key)));
                    double area = frame.Draws.Sum(d => d.ClipRects!.Sum(r =>
                        (double)(r.Z - r.X) * (r.W - r.Y) * d.Step * d.Step));
                    Assert.Equal(2d * 3600 * 3600, area);
                    if (frame.Draws.Length == roots + 3 && !frame.Coverage.Leaves.Contains(region))
                    {
                        Assert.Equal(128 * 128 * 4, probe.Draw(frame.Draws).Length);
                        Console.WriteLine($"[G07/会话压力] 区域={region.X},{region.Y} 容量={capacity} 驻留={probe.Pool.ResidentCount} 上传={probe.Pool.UploadedPages} 收敛={clock.Elapsed.TotalMilliseconds:F1}ms");
                        reached = true;
                        break;
                    }
                }
                else Assert.False(initialized, "已提交覆盖不允许出现空帧。");
                await Task.Delay(10);
            }
            Assert.True(reached, $"G07 压力往返未收敛：{region} 指标={cpu.Diagnostic}");
        }
    }
}