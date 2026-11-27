using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainPageArray : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private Image _image;
    private DeviceMemory _memory;
    private Sampler _sampler;
    private readonly ImageView[] _views;
    public Image Handle => _image;
    public Sampler Sampler => _sampler;
    public ulong AllocatedBytes { get; private set; }
    public ImageView View(int layer) => _views[layer];

    public VkTerrainPageArray(Vk vk, PhysicalDevice gpu, Device device, int capacity, ulong budget)
    {
        _vk = vk; _device = device; _views = new ImageView[capacity];
        try
        {
            vk.GetPhysicalDeviceProperties(gpu, out var props);
            if ((uint)capacity > props.Limits.MaxImageArrayLayers)
                throw new NotSupportedException("G07 高程页数量超过设备数组层数上限。");
            vk.GetPhysicalDeviceFormatProperties(gpu, Format.R16Sint, out var format);
            if ((format.OptimalTilingFeatures & FormatFeatureFlags.SampledImageBit) == 0)
                throw new NotSupportedException("设备不支持 R16_SINT 高程采样。");
            var info = new ImageCreateInfo { SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D, Format = Format.R16Sint,
                Extent = new Extent3D(TerrainPoolUpload.Side, TerrainPoolUpload.PackedRows, 1), MipLevels = 1, ArrayLayers = (uint)capacity,
                Samples = SampleCountFlags.Count1Bit, Tiling = ImageTiling.Optimal,
                Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
                SharingMode = SharingMode.Exclusive, InitialLayout = ImageLayout.Undefined };
            Check(vk.CreateImage(device, &info, null, out _image), "创建 G07 高程数组");
            vk.GetImageMemoryRequirements(device, _image, out var req);
            if (req.Size > budget) throw new InvalidOperationException("G07 高程数组超过显存预算。");
            vk.GetPhysicalDeviceMemoryProperties(gpu, out var memoryProps);
            uint index = 0;
            while (index < memoryProps.MemoryTypeCount && ((req.MemoryTypeBits & (1u << (int)index)) == 0 ||
                (memoryProps.MemoryTypes[(int)index].PropertyFlags & MemoryPropertyFlags.DeviceLocalBit) == 0)) index++;
            if (index == memoryProps.MemoryTypeCount) throw new NotSupportedException("无 G07 高程数组设备显存。");
            var alloc = new MemoryAllocateInfo { SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size, MemoryTypeIndex = index };
            Check(vk.AllocateMemory(device, &alloc, null, out _memory), "分配 G07 高程数组显存");
            Check(vk.BindImageMemory(device, _image, _memory, 0), "绑定 G07 高程数组显存");
            AllocatedBytes = req.Size;
            CreateViews();
        }
        catch { Dispose(); throw; }
    }

    public void Dispose()
    {
        foreach (var view in _views)
            if (view.Handle != 0) _vk.DestroyImageView(_device, view, null);
        Array.Clear(_views);
        if (_sampler.Handle != 0) _vk.DestroySampler(_device, _sampler, null);
        if (_image.Handle != 0) _vk.DestroyImage(_device, _image, null);
        if (_memory.Handle != 0) _vk.FreeMemory(_device, _memory, null);
        _sampler = default; _image = default; _memory = default;
    }
}