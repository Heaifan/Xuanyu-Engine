using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkContext
{
    public TerrainPoolProbeResult ProbeTerrainPagePool(int capacity, int uploadLimit, ulong heightBudget,
        IReadOnlyList<int[]> topology, IReadOnlyList<TerrainPoolFrame> frames)
    {
        if (!IsInitialized) throw new ObjectDisposedException(nameof(VkContext));
        if (!ShaderFloat64Enabled) throw new NotSupportedException("设备不支持 shaderFloat64。");
        ValidatePoolProbe(capacity, uploadLimit, topology, frames);
        VkMsaaSettings.Validate(_vk, PhysicalDevice);
        var extent = new Extent2D(128, 128);
        using var image = new VkTerrainOffscreenImage(_vk, PhysicalDevice, _device, extent);
        using var pass = new VkTrianglePass(_vk, _device, VkTerrainOffscreenImage.ColorFormat, true);
        using var targets = new VkTriangleTargets(_vk, PhysicalDevice, _device, extent,
            VkTerrainOffscreenImage.ColorFormat, [image.Handle], pass.Handle);
        using var pipeline = new VkTerrainDrawPipeline(_vk, _device, pass.Handle, extent);
        using var indices = new VkTerrainTopology(_vk, PhysicalDevice, _device, topology);
        using var pool = new VkTerrainPagePool(_vk, PhysicalDevice, _device, GraphicsQueue,
            Gpu.GraphicsFamilyIndex, pipeline.SetLayout, capacity, uploadLimit, heightBudget);
        using var readback = new VkTerrainBuffer(_vk, PhysicalDevice, _device, 128 * 128 * 4,
            BufferUsageFlags.TransferDstBit);
        using var probe = new VkTerrainDrawProbe(_vk, _device, GraphicsQueue, Gpu.GraphicsFamilyIndex);
        var results = new TerrainPoolFrameResult[frames.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            pool.Poll();
            pool.Protect(frame.ProtectedKeys);
            bool[] evicted = frame.EvictKeys.Select(pool.TryEvict).ToArray();
            int submitted = pool.Submit(frame.Uploads);
            int pending = pool.PendingCount, before = pool.ResidentCount;
            bool[] during = frame.EvictBeforePublish.Select(pool.TryEvict).ToArray();
            if (frame.PublishBeforeDraw && !pool.Poll()) pool.WaitForUpload();
            pool.Protect(frame.Draws.Select(d => d.Key));
            var draws = pool.PrepareDraws(frame.Draws);
            byte[] pixels = probe.DrawCommands(pass.Handle, targets[0], extent, image.Handle,
                readback, pipeline, cmd => pool.Draw(_vk, cmd, pipeline.Layout, indices, draws));
            results[i] = new(pixels, submitted, pending, before, pool.ResidentCount, pool.PendingCount,
                pool.UploadBatches, pool.UploadedPages, evicted, during);
            Console.WriteLine($"[G07/页池] 帧={i + 1} 提交={submitted} 待发布={pending} 驻留={pool.ResidentCount} 绘制时待发布={pool.PendingCount} 批次={pool.UploadBatches} 累计页={pool.UploadedPages} 高程显存={pool.HeightBytes}B");
        }
        return new(pool.HeightBytes, results);
    }

    private static void ValidatePoolProbe(int capacity, int limit, IReadOnlyList<int[]> topology,
        IReadOnlyList<TerrainPoolFrame> frames)
    {
        if (capacity < 1 || limit < 1 || limit > capacity) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (topology.Count != 16 || topology.Any(t => t.Length == 0 || t.Length % 3 != 0 ||
            t.Any(index => (uint)index >= 1089))) throw new ArgumentException("G07 需要16组合法拓扑。");
        foreach (var frame in frames)
        {
            if (frame.Uploads.Select(p => p.Key).Distinct().Count() != frame.Uploads.Count ||
                frame.Uploads.Any(p => p.Heights.Length != 1089 || p.Heights.Span.Contains(short.MinValue)))
                throw new ArgumentException("G07 上传要求唯一页键及完整有效高程。");
            if (frame.Draws.Select(p => p.Key).Distinct().Count() != frame.Draws.Count ||
                frame.Draws.Any(p => p.Step <= 0 || (uint)p.Mask > 15))
                throw new ArgumentException("G07 绘制要求唯一页键及合法步长、掩码。");
        }
    }
}