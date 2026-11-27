namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkContext
{
    public VkTerrainLodProbe CreateTerrainLodProbe(int capacity, int uploadLimit,
        ulong heightBudget, IReadOnlyList<int[]> topology)
    {
        if (!IsInitialized) throw new ObjectDisposedException(nameof(VkContext));
        if (!ShaderFloat64Enabled) throw new NotSupportedException("设备不支持 shaderFloat64。");
        ValidatePoolProbe(capacity, uploadLimit, topology, []);
        VkMsaaSettings.Validate(_vk, PhysicalDevice);
        return new(_vk, PhysicalDevice, _device, GraphicsQueue, Gpu.GraphicsFamilyIndex,
            capacity, uploadLimit, heightBudget, topology);
    }
}