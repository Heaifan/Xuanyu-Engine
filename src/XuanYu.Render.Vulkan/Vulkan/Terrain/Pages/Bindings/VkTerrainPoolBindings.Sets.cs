using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainPoolBindings
{
    private void CreateSets(DescriptorSetLayout layout, VkTerrainPageArray image)
    {
        uint count = (uint)_sets.Length;
        DescriptorPoolSize* sizes = stackalloc DescriptorPoolSize[2];
        sizes[0] = new() { Type = DescriptorType.CombinedImageSampler, DescriptorCount = count };
        sizes[1] = new() { Type = DescriptorType.UniformBuffer, DescriptorCount = count };
        var pool = new DescriptorPoolCreateInfo { SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = count, PoolSizeCount = 2, PPoolSizes = sizes };
        Check(_vk.CreateDescriptorPool(_device, &pool, null, out _pool), "创建 G07 共享描述符池");
        WriteDescriptorSet* writes = stackalloc WriteDescriptorSet[2];
        for (int slot = 0; slot < _sets.Length; slot++)
        {
            var alloc = new DescriptorSetAllocateInfo { SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = _pool, DescriptorSetCount = 1, PSetLayouts = &layout };
            Check(_vk.AllocateDescriptorSets(_device, &alloc, out _sets[slot]), "分配 G07 槽位描述符");
            var tex = new DescriptorImageInfo { ImageView = image.View(slot), Sampler = image.Sampler,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal };
            var buffer = new DescriptorBufferInfo { Buffer = _uniform.Handle,
                Offset = (ulong)(slot * _stride), Range = 160 };
            writes[0] = new() { SType = StructureType.WriteDescriptorSet, DstSet = _sets[slot], DstBinding = 0,
                DescriptorCount = 1, DescriptorType = DescriptorType.CombinedImageSampler, PImageInfo = &tex };
            writes[1] = new() { SType = StructureType.WriteDescriptorSet, DstSet = _sets[slot], DstBinding = 1,
                DescriptorCount = 1, DescriptorType = DescriptorType.UniformBuffer, PBufferInfo = &buffer };
            _vk.UpdateDescriptorSets(_device, 2, writes, 0, null);
        }
    }
}