using Xunit;
using XuanYu.Render.Vulkan;

namespace XuanYu.World.Tests;

public sealed class test_VkContext
{
    [Fact]
    public void Vulkan13_ShouldCreateAndReleaseGpuResources()
    {
        var context = new VkContext();

        try
        {
            Assert.True(context.IsInitialized);
            Assert.False(string.IsNullOrWhiteSpace(
                context.Gpu.DeviceName));
            Assert.False(string.IsNullOrWhiteSpace(
                context.Gpu.ApiVersionText));
            Assert.Contains("Vulkan", context.Gpu.Summary);
        }
        finally
        {
            context.Dispose();
            context.Dispose();
        }

        Assert.False(context.IsInitialized);
    }
}