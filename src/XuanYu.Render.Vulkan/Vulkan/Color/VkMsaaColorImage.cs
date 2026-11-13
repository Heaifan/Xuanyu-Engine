using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkMsaaColorImage : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private Image _image;
    private DeviceMemory _memory;
    private ImageView _view;
    public ImageView View => _view;

    public VkMsaaColorImage(Vk vk, PhysicalDevice gpu, Device device,
        Extent2D extent, Format format)
    {
        _vk = vk;
        _device = device;
        try
        {
            var info = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent = new Extent3D(extent.Width, extent.Height, 1),
                MipLevels = 1, ArrayLayers = 1, Format = format,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.TransientAttachmentBit |
                    ImageUsageFlags.ColorAttachmentBit,
                Samples = VkMsaaSettings.Samples,
                SharingMode = SharingMode.Exclusive
            };
            Check(vk.CreateImage(device, &info, null, out _image), "创建 MSAA 颜色图像");
            vk.GetImageMemoryRequirements(device, _image, out var requirements);
            vk.GetPhysicalDeviceMemoryProperties(gpu, out var properties);
            var allocate = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = requirements.Size,
                MemoryTypeIndex = FindMemory(properties, requirements.MemoryTypeBits)
            };
            Check(vk.AllocateMemory(device, &allocate, null, out _memory), "分配 MSAA 显存");
            Check(vk.BindImageMemory(device, _image, _memory, 0), "绑定 MSAA 显存");
            var view = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo, Image = _image,
                ViewType = ImageViewType.Type2D, Format = format,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    LevelCount = 1, LayerCount = 1
                }
            };
            Check(vk.CreateImageView(device, &view, null, out _view), "创建 MSAA 颜色视图");
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
        throw new NotSupportedException("GPU 没有可用的 MSAA 设备显存类型。");
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