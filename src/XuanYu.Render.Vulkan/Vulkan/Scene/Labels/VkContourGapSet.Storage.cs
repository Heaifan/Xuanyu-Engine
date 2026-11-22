using System.Numerics;
using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkContourGapSet
{
    private void EnsureCapacity(int count)
    {
        if (count <= _capacity) return;
        int capacity = Math.Max(256, _capacity);
        while (capacity < count) capacity *= 2;
        DestroyStorage();
        _capacity = capacity;
        ulong bytes = (ulong)capacity * (ulong)sizeof(Vector4);
        var info = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = bytes, Usage = BufferUsageFlags.StorageBufferBit,
            SharingMode = SharingMode.Exclusive
        };
        Check(_vk.CreateBuffer(_device, &info, null, out _buffer));
        _vk.GetBufferMemoryRequirements(_device, _buffer, out var requirements);
        var allocate = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = FindMemory(requirements.MemoryTypeBits)
        };
        Check(_vk.AllocateMemory(_device, &allocate, null, out _memory));
        Check(_vk.BindBufferMemory(_device, _buffer, _memory, 0));
        var buffer = new DescriptorBufferInfo
            { Buffer = _buffer, Offset = 0, Range = bytes };
        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = Set, DstBinding = 0,
            DescriptorCount = 1, DescriptorType = DescriptorType.StorageBuffer,
            PBufferInfo = &buffer
        };
        _vk.UpdateDescriptorSets(_device, 1, &write, 0, null);
    }

    private uint FindMemory(uint allowed)
    {
        _vk.GetPhysicalDeviceMemoryProperties(_gpu, out var props);
        const MemoryPropertyFlags wanted =
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit;
        for (uint i = 0; i < props.MemoryTypeCount; i++)
            if ((allowed & (1u << (int)i)) != 0 &&
                (props.MemoryTypes[(int)i].PropertyFlags & wanted) == wanted)
                return i;
        throw new NotSupportedException("未找到可用的 HostCoherent 标高缓冲内存。");
    }

    private void DestroyStorage()
    {
        if (_buffer.Handle != 0) _vk.DestroyBuffer(_device, _buffer, null);
        if (_memory.Handle != 0) _vk.FreeMemory(_device, _memory, null);
        _buffer = default; _memory = default; _capacity = 0;
    }
}