using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTerrainOffscreenImage : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private Image _image;
    private DeviceMemory _memory;
    public Image Handle => _image;
    public const Format ColorFormat = Format.R8G8B8A8Unorm;

    public VkTerrainOffscreenImage(Vk vk, PhysicalDevice gpu, Device device, Extent2D extent)
    {
        _vk = vk; _device = device;
        try
        {
            var info = new ImageCreateInfo { SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D, Format = ColorFormat,
                Extent = new Extent3D(extent.Width, extent.Height, 1), MipLevels = 1, ArrayLayers = 1,
                Samples = SampleCountFlags.Count1Bit, Tiling = ImageTiling.Optimal,
                Usage = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferSrcBit,
                SharingMode = SharingMode.Exclusive, InitialLayout = ImageLayout.Undefined };
            Check(vk.CreateImage(device, &info, null, out _image), "创建 G07 诊断图像");
            vk.GetImageMemoryRequirements(device, _image, out var req);
            vk.GetPhysicalDeviceMemoryProperties(gpu, out var props);
            uint index = 0;
            while (index < props.MemoryTypeCount && ((req.MemoryTypeBits & (1u << (int)index)) == 0 ||
                (props.MemoryTypes[(int)index].PropertyFlags & MemoryPropertyFlags.DeviceLocalBit) == 0)) index++;
            if (index == props.MemoryTypeCount) throw new NotSupportedException("无 G07 诊断图像显存。");
            var alloc = new MemoryAllocateInfo { SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size, MemoryTypeIndex = index };
            Check(vk.AllocateMemory(device, &alloc, null, out _memory), "分配 G07 诊断图像显存");
            Check(vk.BindImageMemory(device, _image, _memory, 0), "绑定 G07 诊断图像");
        }
        catch { Dispose(); throw; }
    }

    public void Dispose()
    {
        if (_image.Handle != 0) _vk.DestroyImage(_device, _image, null);
        if (_memory.Handle != 0) _vk.FreeMemory(_device, _memory, null);
        _image = default; _memory = default;
    }
}