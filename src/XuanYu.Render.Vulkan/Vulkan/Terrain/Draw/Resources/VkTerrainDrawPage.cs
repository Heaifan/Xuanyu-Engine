using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTerrainDrawPage : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private VkDemTexture? _texture;
    private VkDemTexture? _water;
    private VkTerrainBuffer? _uniform;
    private DescriptorPool _pool;
    private DescriptorSet _set;

    public VkTerrainDrawPage(Vk vk, PhysicalDevice gpu, Device device, Queue queue, uint family,
        DescriptorSetLayout layout, ReadOnlyMemory<short> heights, ReadOnlySpan<byte> parameters)
    {
        _vk = vk; _device = device;
        try
        {
            _texture = VkDemTexture.FromTerrainPage(vk, gpu, device, queue, family, heights);
            _uniform = new VkTerrainBuffer(vk, gpu, device, VkTerrainDrawParameters.ByteSize, BufferUsageFlags.UniformBufferBit);
            _uniform.Write(parameters);
            _water = VkDemTexture.Fallback(vk, gpu, device, queue, family);
            CreateSet(layout, _texture, _uniform, _water);
        }
        catch { Dispose(); throw; }
    }

    private void CreateSet(DescriptorSetLayout layout, VkDemTexture texture, VkTerrainBuffer uniform, VkDemTexture water)
    {
        DescriptorPoolSize* sizes = stackalloc DescriptorPoolSize[2];
        sizes[0] = new() { Type = DescriptorType.CombinedImageSampler, DescriptorCount = 2 };
        sizes[1] = new() { Type = DescriptorType.UniformBuffer, DescriptorCount = 1 };
        var pool = new DescriptorPoolCreateInfo { SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = 1, PoolSizeCount = 2, PPoolSizes = sizes };
        Check(_vk.CreateDescriptorPool(_device, &pool, null, out _pool), "创建 G07 页描述符池");
        var alloc = new DescriptorSetAllocateInfo { SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _pool, DescriptorSetCount = 1, PSetLayouts = &layout };
        Check(_vk.AllocateDescriptorSets(_device, &alloc, out _set), "分配 G07 页描述符集");
        var image = new DescriptorImageInfo { ImageView = texture.View, Sampler = texture.Sampler,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal };
        var buffer = new DescriptorBufferInfo { Buffer = uniform.Handle, Range = VkTerrainDrawParameters.ByteSize };
        WriteDescriptorSet* writes = stackalloc WriteDescriptorSet[3];
        writes[0] = new() { SType = StructureType.WriteDescriptorSet, DstSet = _set, DstBinding = 0,
            DescriptorCount = 1, DescriptorType = DescriptorType.CombinedImageSampler, PImageInfo = &image };
        writes[1] = new() { SType = StructureType.WriteDescriptorSet, DstSet = _set, DstBinding = 1,
            DescriptorCount = 1, DescriptorType = DescriptorType.UniformBuffer, PBufferInfo = &buffer };
        var swb = new DescriptorImageInfo { ImageView = water.View, Sampler = water.Sampler,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal };
        writes[2] = new() { SType = StructureType.WriteDescriptorSet, DstSet = _set, DstBinding = 2,
            DescriptorCount = 1, DescriptorType = DescriptorType.CombinedImageSampler, PImageInfo = &swb };
        _vk.UpdateDescriptorSets(_device, 3, writes, 0, null);
    }

    public void Bind(CommandBuffer command, PipelineLayout layout)
    {
        var set = _set;
        _vk.CmdBindDescriptorSets(command, PipelineBindPoint.Graphics, layout, 0, 1, &set, 0, null);
    }

    public void Dispose()
    {
        if (_pool.Handle != 0) _vk.DestroyDescriptorPool(_device, _pool, null);
        _pool = default;
        _uniform?.Dispose(); _uniform = null;
        _water?.Dispose(); _water = null;
        _texture?.Dispose(); _texture = null;
    }
}