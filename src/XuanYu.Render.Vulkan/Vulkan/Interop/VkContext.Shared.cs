namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkContext
{
    public VkSharedColorProbe CreateSharedColorProbe(nint ntHandle, uint width, uint height)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("此验证用例仅支持 Windows");
        return new VkSharedColorProbe(_vk, _instance, PhysicalDevice,
            _device, GraphicsQueue, Gpu.GraphicsFamilyIndex, ntHandle, width, height);
    }
}