using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkContext
{
    public Vector3[] ProbeTerrainCoordinates(int south, int west, int step,
        GeoCoordinate origin, ReadOnlySpan<short> heights)
    {
        if (!IsInitialized) throw new ObjectDisposedException(nameof(VkContext));
        if (!ShaderFloat64Enabled) throw new NotSupportedException("设备不支持 shaderFloat64。");
        if (step <= 0 || heights.Length != 33 * 33)
            throw new ArgumentOutOfRangeException(nameof(step));
        using var probe = new VkTerrainCoordinateProbe(_vk, PhysicalDevice, _device,
            GraphicsQueue, Gpu.GraphicsFamilyIndex);
        return probe.Run(south, west, step, origin, heights);
    }
}
