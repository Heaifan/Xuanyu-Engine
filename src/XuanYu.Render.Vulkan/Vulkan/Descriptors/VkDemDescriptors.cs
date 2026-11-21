using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkDemDescriptors : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private DescriptorPool _pool;
    private DescriptorSetLayout _layout;
    private readonly Dictionary<VkDemTexture, DescriptorSet> _sets = [];
    public DescriptorSetLayout Layout => _layout;

    public VkDemDescriptors(Vk vk, Device device)
    {
        _vk = vk;
        _device = device;
        try
        {
            var binding = new DescriptorSetLayoutBinding
            {
                Binding = 0, DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                StageFlags = ShaderStageFlags.FragmentBit
            };
            var layout = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1, PBindings = &binding
            };
            Check(vk.CreateDescriptorSetLayout(device, &layout, null, out _layout));
            var size = new DescriptorPoolSize
                { Type = DescriptorType.CombinedImageSampler, DescriptorCount = 14 };
            var pool = new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit,
                MaxSets = 14, PoolSizeCount = 1, PPoolSizes = &size
            };
            Check(vk.CreateDescriptorPool(device, &pool, null, out _pool));
        }
        catch { Dispose(); throw; }
    }

    public DescriptorSet Get(VkDemTexture texture)
    {
        if (_sets.TryGetValue(texture, out var existing)) return existing;
        DescriptorSetLayout layout = _layout;
        var alloc = new DescriptorSetAllocateInfo
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _pool, DescriptorSetCount = 1,
            PSetLayouts = &layout
        };
        Check(_vk.AllocateDescriptorSets(_device, &alloc, out var set));
        var image = new DescriptorImageInfo
        {
            Sampler = texture.Sampler, ImageView = texture.View,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal
        };
        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = set, DstBinding = 0, DescriptorCount = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            PImageInfo = &image
        };
        _vk.UpdateDescriptorSets(_device, 1, &write, 0, null);
        _sets.Add(texture, set);
        return set;
    }

    public void Release(VkDemTexture texture)
    {
        if (!_sets.Remove(texture, out var set)) return;
        Check(_vk.FreeDescriptorSets(_device, _pool, 1, &set));
    }

    private static void Check(Result result)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"DEM 描述符操作失败：{result}");
    }

    public void Dispose()
    {
        _sets.Clear();
        if (_pool.Handle != 0) _vk.DestroyDescriptorPool(_device, _pool, null);
        if (_layout.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _layout, null);
        _pool = default;
        _layout = default;
    }
}