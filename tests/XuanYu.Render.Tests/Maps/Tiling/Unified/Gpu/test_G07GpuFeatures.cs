using Silk.NET.Vulkan;
using XuanYu.Render.Vulkan;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07GpuFeatures
{
    [Fact]
    public void ContextEnablesFloat64OnlyWhenPhysicalDeviceSupportsIt()
    {
        using var context = new VkContext();
        using var vk = Vk.GetApi();
        vk.GetPhysicalDeviceFeatures(context.PhysicalDevice, out var features);
        Assert.Equal(features.ShaderFloat64.Value != 0, context.ShaderFloat64Enabled);
    }
}