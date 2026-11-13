using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkDepthImage : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private Image _image;
    private DeviceMemory _memory;
    private ImageView _view;
    public ImageView View => _view;
    public const Format DepthFormat = Format.D32Sfloat;

    public VkDepthImage(Vk vk, PhysicalDevice gpu, Device device, Extent2D extent)
    {
        _vk = vk;
        _device = device;
        vk.GetPhysicalDeviceFormatProperties(gpu, DepthFormat, out var format);
        if ((format.OptimalTilingFeatures & FormatFeatureFlags.DepthStencilAttachmentBit) == 0)
            throw new NotSupportedException("GPU 不支持 D32Sfloat 深度附件。");
        try
        {
            var info = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent = new Extent3D(extent.Width, extent.Height, 1),
                MipLevels = 1, ArrayLayers = 1,
                Format = DepthFormat, Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.DepthStencilAttachmentBit,
                Samples = VkMsaaSettings.Samples,
                SharingMode = SharingMode.Exclusive
            };
            Check(vk.CreateImage(device, &info, null, out _image), "创建深度图像");
            vk.GetImageMemoryRequirements(device, _image, out var requirements);
            vk.GetPhysicalDeviceMemoryProperties(gpu, out var properties);
            var alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = requirements.Size,
                MemoryTypeIndex = FindMemory(properties, requirements.MemoryTypeBits)
            };
            Check(vk.AllocateMemory(device, &alloc, null, out _memory), "分配深度显存");
            Check(vk.BindImageMemory(device, _image, _memory, 0), "绑定深度显存");
            var viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _image, ViewType = ImageViewType.Type2D,
                Format = DepthFormat,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.DepthBit,
                    LevelCount = 1, LayerCount = 1
                }
            };
            Check(vk.CreateImageView(device, &viewInfo, null, out _view), "创建深度视图");
        }
        catch { Dispose(); throw; }
    }

    private static uint FindMemory(PhysicalDeviceMemoryProperties props, uint bits)
    {
        for (uint i = 0; i < props.MemoryTypeCount; i++)
            if ((bits & (1u << (int)i)) != 0 &&
                (props.MemoryTypes[(int)i].PropertyFlags &
                 MemoryPropertyFlags.DeviceLocalBit) != 0)
                return i;
        throw new NotSupportedException("GPU 缺少可用的深度显存类型。");
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }

    public void Dispose()
    {
        if (_view.Handle != 0) _vk.DestroyImageView(_device, _view, null);
        if (_image.Handle != 0) _vk.DestroyImage(_device, _image, null);
        if (_memory.Handle != 0) _vk.FreeMemory(_device, _memory, null);
        _view = default; _image = default; _memory = default;
    }
}