
using Silk.NET.Vulkan;
using XuanYu.Render;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkMeshBuffer : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private VkBuffer _buffer;
    private DeviceMemory _memory;
    public VkBuffer Handle => _buffer;

    public VkMeshBuffer(Vk vk, PhysicalDevice gpu, Device device, RenderMesh mesh)
    {
        _vk = vk;
        _device = device;
        ulong bytes = (ulong)mesh.Vertices.Length * sizeof(float);
        try
        {
            var info = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = bytes,
                Usage = BufferUsageFlags.VertexBufferBit,
                SharingMode = SharingMode.Exclusive
            };
            Check(vk.CreateBuffer(device, &info, null, out _buffer),
                "创建顶点缓冲区");
            vk.GetBufferMemoryRequirements(device, _buffer, out var requirements);
            var allocate = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = requirements.Size,
                MemoryTypeIndex = FindMemoryType(
                    vk, gpu, requirements.MemoryTypeBits)
            };
            Check(vk.AllocateMemory(device, &allocate, null, out _memory),
                "分配顶点显存");
            Check(vk.BindBufferMemory(device, _buffer, _memory, 0),
                "绑定顶点显存");
            void* mapped = null;
            Check(vk.MapMemory(device, _memory, 0, bytes, 0, &mapped),
                "映射顶点显存");
            fixed (float* source = mesh.Vertices.Span)
                System.Buffer.MemoryCopy(
                    source, mapped, (long)bytes, (long)bytes);
            vk.UnmapMemory(device, _memory);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private static uint FindMemoryType(
        Vk vk, PhysicalDevice gpu, uint allowed)
    {
        vk.GetPhysicalDeviceMemoryProperties(gpu, out var props);
        const MemoryPropertyFlags wanted =
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit;
        for (uint i = 0; i < props.MemoryTypeCount; i++)
        {
            if ((allowed & (1u << (int)i)) != 0 &&
                (props.MemoryTypes[(int)i].PropertyFlags & wanted) == wanted)
                return i;
        }
        throw new NotSupportedException(
            "没有可用的 HostVisible/HostCoherent 顶点内存。");
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }

    public void Dispose()
    {
        if (_buffer.Handle != 0)
            _vk.DestroyBuffer(_device, _buffer, null);
        if (_memory.Handle != 0)
            _vk.FreeMemory(_device, _memory, null);
        _buffer = default;
        _memory = default;
    }
}
