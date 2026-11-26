using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainCoordinateProbe
{
    private void CreateBuffers()
    {
        CreateBuffer(96 + 1089 * sizeof(int), out _input, out _inputMemory);
        CreateBuffer(1089 * 16, out _output, out _outputMemory);
    }

    private void CreateBuffer(ulong size, out VkBuffer buffer, out DeviceMemory memory)
    {
        var info = new BufferCreateInfo { SType = StructureType.BufferCreateInfo,
            Size = size, Usage = BufferUsageFlags.StorageBufferBit,
            SharingMode = SharingMode.Exclusive };
        Check(_vk.CreateBuffer(_device, &info, null, out buffer), "创建地形诊断缓冲区");
        _vk.GetBufferMemoryRequirements(_device, buffer, out var req);
        _vk.GetPhysicalDeviceMemoryProperties(_physical, out var props);
        uint index = uint.MaxValue;
        for (uint i = 0; i < props.MemoryTypeCount; i++)
            if ((req.MemoryTypeBits & (1u << (int)i)) != 0 &&
                (props.MemoryTypes[(int)i].PropertyFlags &
                 (MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)) ==
                 (MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit))
            { index = i; break; }
        if (index == uint.MaxValue) throw new NotSupportedException("无 HostCoherent 缓冲区内存。");
        var alloc = new MemoryAllocateInfo { SType = StructureType.MemoryAllocateInfo,
            AllocationSize = req.Size, MemoryTypeIndex = index };
        Check(_vk.AllocateMemory(_device, &alloc, null, out memory), "分配地形诊断内存");
        Check(_vk.BindBufferMemory(_device, buffer, memory, 0), "绑定地形诊断内存");
    }

    private static void Check(Result result, string action)
    { if (result != Result.Success) throw new InvalidOperationException($"{action}: {result}"); }
}
