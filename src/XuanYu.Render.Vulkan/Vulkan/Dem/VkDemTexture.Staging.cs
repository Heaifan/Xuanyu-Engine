using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkDemTexture
{
    private void Upload(PhysicalDevice gpu, Queue queue, uint family,
        ReadOnlyMemory<short> samples)
    {
        ulong bytes = (ulong)samples.Length * sizeof(short);
        VkBuffer staging = default;
        DeviceMemory memory = default;
        try
        {
            var info = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = bytes,
                Usage = BufferUsageFlags.TransferSrcBit,
                SharingMode = SharingMode.Exclusive
            };
            Check(_vk.CreateBuffer(_device, &info, null, out staging),
                "创建 DEM 传输缓冲区");
            _vk.GetBufferMemoryRequirements(_device, staging, out var required);
            var allocate = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = required.Size,
                MemoryTypeIndex = FindMemory(gpu, required.MemoryTypeBits,
                    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
            };
            Check(_vk.AllocateMemory(_device, &allocate, null, out memory),
                "分配 DEM 传输内存");
            Check(_vk.BindBufferMemory(_device, staging, memory, 0),
                "绑定 DEM 传输内存");
            void* mapped = null;
            Check(_vk.MapMemory(_device, memory, 0, bytes, 0, &mapped),
                "映射 DEM 传输内存");
            fixed (short* source = samples.Span)
                System.Buffer.MemoryCopy(source, mapped, (long)bytes, (long)bytes);
            _vk.UnmapMemory(_device, memory);
            Transfer(queue, family, staging);
        }
        finally
        {
            if (staging.Handle != 0)
                _vk.DestroyBuffer(_device, staging, null);
            if (memory.Handle != 0)
                _vk.FreeMemory(_device, memory, null);
        }
    }
}