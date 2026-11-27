using System.Diagnostics;
using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

internal sealed class G07GpuLodDriver(GeoUnifiedGpuSession session, VkTerrainLodProbe probe,
    Matrix4x4 mvp, GeoUnifiedTile root)
{
    public GeoUnifiedGpuFrame Tick(EnuPosition eye)
    {
        var frame = session.Step(eye, 1080, Math.PI / 3, mvp);
        Assert.InRange(frame.SubmittedPages, 0, 1);
        Assert.InRange(probe.Pool.ResidentCount + probe.Pool.PendingCount, 0, probe.Pool.Capacity);
        if (frame.Draws.Length == 0) return frame;
        Assert.Equal(frame.Coverage.Leaves.Count, frame.Draws.Length);
        Assert.All(frame.Draws, draw => Assert.True(probe.Pool.IsReady(draw.Key)));
        if (frame.Draws.Length == 1) Assert.Equal(root, Assert.Single(frame.Coverage.Leaves));
        else Assert.Equal(4, frame.Draws.Length);
        byte[] pixels = probe.Draw(frame.Draws);
        for (int y = 32; y < 96; y++)
            for (int x = 32; x < 96; x++)
                Assert.True(pixels[(y * 128 + x) * 4 + 1] >= 75, $"G07 覆盖内部空洞：{x},{y}");
        return frame;
    }

    public async Task Until(EnuPosition eye, Func<GeoUnifiedGpuFrame, bool> complete, int previousCount)
    {
        var clock = Stopwatch.StartNew();
        bool observedPending = false;
        while (clock.Elapsed < TimeSpan.FromSeconds(30))
        {
            var frame = Tick(eye);
            if (frame.PendingCoverage is not null && frame.Draws.Length == previousCount)
                observedPending = true;
            if (complete(frame))
            {
                if (previousCount != 0) Assert.True(observedPending, "未观察到旧覆盖保护阶段。");
                Console.WriteLine($"[G07/LOD联动] 覆盖={frame.Draws.Length} 阻塞={frame.CapacityBlocked} 驻留={probe.Pool.ResidentCount} 累计上传={probe.Pool.UploadedPages}");
                return;
            }
            Assert.Equal(previousCount, frame.Draws.Length);
            await Task.Delay(10);
        }
        Assert.Fail($"G07 LOD 联动未完成：上一覆盖={previousCount} 驻留={probe.Pool.ResidentCount} 待发布={probe.Pool.PendingCount}");
    }
}