using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTerrainBuffer : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly ulong _size;
    private VkBuffer _buffer;
    private DeviceMemory _memory;
    public VkBuffer Handle => _buffer;

    public VkTerrainBuffer(Vk vk, PhysicalDevice gpu, Device device, ulong size, BufferUsageFlags usage)
    {
        _vk = vk; _device = device; _size = size;
        try
        {
            var info = new BufferCreateInfo { SType = StructureType.BufferCreateInfo,
                Size = size, Usage = usage, SharingMode = SharingMode.Exclusive };
            Check(vk.CreateBuffer(device, &info, null, out _buffer), "创建 G07 缓冲区");
            vk.GetBufferMemoryRequirements(device, _buffer, out var req);
            vk.GetPhysicalDeviceMemoryProperties(gpu, out var props);
            const MemoryPropertyFlags wanted = MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit;
            uint index = 0;
            while (index < props.MemoryTypeCount && ((req.MemoryTypeBits & (1u << (int)index)) == 0 ||
                (props.MemoryTypes[(int)index].PropertyFlags & wanted) != wanted)) index++;
            if (index == props.MemoryTypeCount) throw new NotSupportedException("无 G07 可映射缓冲区内存。");
            var alloc = new MemoryAllocateInfo { SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size, MemoryTypeIndex = index };
            Check(vk.AllocateMemory(device, &alloc, null, out _memory), "分配 G07 缓冲区内存");
            Check(vk.BindBufferMemory(device, _buffer, _memory, 0), "绑定 G07 缓冲区内存");
        }
        catch { Dispose(); throw; }
    }

    public void Write(ReadOnlySpan<byte> bytes)
    {
        void* mapped = null;
        Check(_vk.MapMemory(_device, _memory, 0, _size, 0, &mapped), "映射 G07 写入");
        try { bytes.CopyTo(new Span<byte>(mapped, checked((int)_size))); }
        finally { _vk.UnmapMemory(_device, _memory); }
    }

    public byte[] Read()
    {
        void* mapped = null;
        Check(_vk.MapMemory(_device, _memory, 0, _size, 0, &mapped), "映射 G07 读回");
        try { return new ReadOnlySpan<byte>(mapped, checked((int)_size)).ToArray(); }
        finally { _vk.UnmapMemory(_device, _memory); }
    }

    internal static void Check(Result result, string action)
    {
        if (result != Result.Success) throw new InvalidOperationException($"{action}：{result}");
    }

    public void Dispose()
    {
        if (_buffer.Handle != 0) _vk.DestroyBuffer(_device, _buffer, null);
        if (_memory.Handle != 0) _vk.FreeMemory(_device, _memory, null);
        _buffer = default; _memory = default;
    }
}