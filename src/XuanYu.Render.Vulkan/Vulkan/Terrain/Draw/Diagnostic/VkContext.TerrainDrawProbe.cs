using System.Numerics;
using Silk.NET.Vulkan;
using XuanYu.Core.Geodesy;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkContext
{
    public byte[][] ProbeTerrainDraws(int south, int west, int step, GeoCoordinate origin,
        ReadOnlySpan<short> heights, Matrix4x4 mvp, IReadOnlyList<int[]> topology, IReadOnlyList<int> masks)
    {
        if (!IsInitialized) throw new ObjectDisposedException(nameof(VkContext));
        if (!ShaderFloat64Enabled) throw new NotSupportedException("设备不支持 shaderFloat64。");
        if (step <= 0 || heights.Length != 1089 || heights.Contains(short.MinValue))
            throw new ArgumentException("G07 绘制要求完整有效的 33×33 高程页。");
        if (topology.Count != 16 || topology.Any(indices => indices.Length == 0 ||
            indices.Length % 3 != 0 || indices.Any(index => (uint)index >= 1089)))
            throw new ArgumentException("G07 绘制要求 16 组合法索引拓扑。", nameof(topology));
        if (masks.Count == 0 || masks.Any(mask => (uint)mask > 15))
            throw new ArgumentException("G07 收边掩码必须在 0～15。", nameof(masks));
        VkMsaaSettings.Validate(_vk, PhysicalDevice);
        var extent = new Extent2D(128, 128);
        using var image = new VkTerrainOffscreenImage(_vk, PhysicalDevice, _device, extent);
        using var pass = new VkTrianglePass(_vk, _device, VkTerrainOffscreenImage.ColorFormat, true);
        using var targets = new VkTriangleTargets(_vk, PhysicalDevice, _device, extent,
            VkTerrainOffscreenImage.ColorFormat, [image.Handle], pass.Handle);
        using var pipeline = new VkTerrainDrawPipeline(_vk, _device, pass.Handle, extent);
        using var indices = new VkTerrainTopology(_vk, PhysicalDevice, _device, topology);
        using var page = new VkTerrainDrawPage(_vk, PhysicalDevice, _device, GraphicsQueue,
            Gpu.GraphicsFamilyIndex, pipeline.SetLayout, heights.ToArray(),
            VkTerrainDrawParameters.Create(south, west, step, origin, mvp));
        using var readback = new VkTerrainBuffer(_vk, PhysicalDevice, _device, 128 * 128 * 4,
            BufferUsageFlags.TransferDstBit);
        using var probe = new VkTerrainDrawProbe(_vk, _device, GraphicsQueue, Gpu.GraphicsFamilyIndex);
        var frames = new byte[masks.Count][];
        for (int i = 0; i < masks.Count; i++)
            frames[i] = probe.Draw(pass.Handle, targets[0], extent, image.Handle,
                readback, pipeline, page, indices, masks[i]);
        Console.WriteLine($"[G07/绘制诊断] 帧={frames.Length} 单页复用 共享索引复用 像素读回={128 * 128 * 4}字节/帧");
        return frames;
    }
}