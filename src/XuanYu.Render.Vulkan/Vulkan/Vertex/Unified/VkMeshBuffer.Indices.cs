using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkMeshBuffer
{
    private void CreateIndices(PhysicalDevice gpu, ReadOnlySpan<uint> indices)
    {
        if (indices.IsEmpty) return;
        IndexCount = (uint)indices.Length;
        ulong bytes = (ulong)indices.Length * sizeof(uint);
        var create = new BufferCreateInfo { SType = StructureType.BufferCreateInfo,
            Size = bytes, Usage = BufferUsageFlags.IndexBufferBit,
            SharingMode = SharingMode.Exclusive };
        Check(_vk.CreateBuffer(_device, &create, null, out _indexBuffer), "创建地形索引缓冲区");
        _vk.GetBufferMemoryRequirements(_device, _indexBuffer, out var req);
        var alloc = new MemoryAllocateInfo { SType = StructureType.MemoryAllocateInfo,
            AllocationSize = req.Size, MemoryTypeIndex = FindMemoryType(_vk, gpu, req.MemoryTypeBits) };
        Check(_vk.AllocateMemory(_device, &alloc, null, out _indexMemory), "分配地形索引内存");
        Check(_vk.BindBufferMemory(_device, _indexBuffer, _indexMemory, 0), "绑定地形索引内存");
        void* mapped = null;
        Check(_vk.MapMemory(_device, _indexMemory, 0, bytes, 0, &mapped), "映射地形索引内存");
        try
        {
            fixed (uint* source = indices)
                System.Buffer.MemoryCopy(source, mapped, (long)bytes, (long)bytes);
        }
        finally { _vk.UnmapMemory(_device, _indexMemory); }
    }
}
