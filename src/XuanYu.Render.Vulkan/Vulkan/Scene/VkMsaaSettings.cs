using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal static class VkMsaaSettings
{
    public const SampleCountFlags Samples = SampleCountFlags.Count4Bit;

    public static void Validate(Vk vk, PhysicalDevice gpu)
    {
        vk.GetPhysicalDeviceProperties(gpu, out var properties);
        var available = properties.Limits.FramebufferColorSampleCounts &
            properties.Limits.FramebufferDepthSampleCounts;
        if ((available & Samples) != Samples)
            throw new NotSupportedException("当前 Vulkan GPU 不支持颜色和深度 4x MSAA。");
    }
}