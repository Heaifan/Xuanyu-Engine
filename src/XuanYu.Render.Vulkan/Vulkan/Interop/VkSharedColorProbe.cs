using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkSharedColorProbe : IDisposable
{
    private readonly Vk _vk;
    private readonly PhysicalDevice _gpu;
    private readonly Device _device;
    private readonly Queue _queue;
    private Image _image;
    private DeviceMemory _memory;
    private CommandPool _pool;
    private CommandBuffer _command;
    private Fence _fence;

    internal VkSharedColorProbe(Vk vk, Instance instance, PhysicalDevice gpu,
        Device device, Queue queue, uint family, nint handle, uint width, uint height)
    {
        _vk = vk; _gpu = gpu; _device = device; _queue = queue;
        if (!vk.TryGetDeviceExtension<KhrExternalMemoryWin32>(instance, device, out var api))
            throw new NotSupportedException("VK_KHR_external_memory_win32 不可用");
        var external = new ExternalMemoryImageCreateInfo
        {
            SType = StructureType.ExternalMemoryImageCreateInfo,
            HandleTypes = ExternalMemoryHandleTypeFlags.D3D11TextureBit
        };
        var imageInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo, PNext = &external,
            ImageType = ImageType.Type2D,
            Format = Format.R8G8B8A8Unorm,
            Extent = new Extent3D(width, height, 1),
            MipLevels = 1, ArrayLayers = 1, Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.ColorAttachmentBit,
            SharingMode = SharingMode.Exclusive
        };
        Check(vk.CreateImage(device, &imageInfo, null, out _image), "创建 Vulkan 共享图像");
        vk.GetImageMemoryRequirements(device, _image, out var requirements);
        Check(api.GetMemoryWin32HandleProperties(device,
            ExternalMemoryHandleTypeFlags.D3D11TextureBit, handle, out var props),
            "查询 D3D11 外部显存");
        var import = new ImportMemoryWin32HandleInfoKHR
        {
            SType = StructureType.ImportMemoryWin32HandleInfoKhr,
            HandleType = ExternalMemoryHandleTypeFlags.D3D11TextureBit,
            Handle = handle
        };
        var alloc = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo, PNext = &import,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = FindMemory(requirements.MemoryTypeBits & props.MemoryTypeBits)
        };
        Check(vk.AllocateMemory(device, &alloc, null, out _memory), "导入共享显存");
        Check(vk.BindImageMemory(device, _image, _memory, 0), "绑定共享显存");
        CreateCommands(family);
    }

    private uint FindMemory(uint allowed)
    {
        _vk.GetPhysicalDeviceMemoryProperties(_gpu, out var properties);
        for (uint i = 0; i < properties.MemoryTypeCount; i++)
            if ((allowed & (1u << (int)i)) != 0 &&
                (properties.MemoryTypes[(int)i].PropertyFlags &
                MemoryPropertyFlags.DeviceLocalBit) != 0) return i;
        throw new NotSupportedException("外部共享纹理没有兼容的显存类型");
    }

    private static void Check(Result result, string operation)
    {
        if (result != Result.Success) throw new InvalidOperationException(
            $"{operation}失败：{result}");
    }
}