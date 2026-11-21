using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkDemTexture
{
    private void CreateImage(PhysicalDevice gpu)
    {
        _vk.GetPhysicalDeviceFormatProperties(gpu, Format.R16Sint, out var format);
        if ((format.OptimalTilingFeatures & FormatFeatureFlags.SampledImageBit) == 0)
            throw new NotSupportedException("GPU 不支持 R16_SINT 原始 DEM 纹理采样。");
        var info = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = Format.R16Sint,
            Extent = new Extent3D(_side, _side, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined
        };
        Check(_vk.CreateImage(_device, &info, null, out _image),
            "创建 R16_SINT 高程纹理");
        _vk.GetImageMemoryRequirements(_device, _image, out var requirements);
        var allocate = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = FindMemory(gpu, requirements.MemoryTypeBits,
                MemoryPropertyFlags.DeviceLocalBit)
        };
        Check(_vk.AllocateMemory(_device, &allocate, null, out _memory),
            "分配高程纹理显存");
        Check(_vk.BindImageMemory(_device, _image, _memory, 0),
            "绑定高程纹理显存");
    }

    private uint FindMemory(PhysicalDevice gpu, uint allowed,
        MemoryPropertyFlags wanted)
    {
        _vk.GetPhysicalDeviceMemoryProperties(gpu, out var properties);
        for (uint i = 0; i < properties.MemoryTypeCount; i++)
            if ((allowed & (1u << (int)i)) != 0 &&
                (properties.MemoryTypes[(int)i].PropertyFlags & wanted) == wanted)
                return i;
        throw new NotSupportedException($"缺少高程纹理兼容显存：{wanted}");
    }
}