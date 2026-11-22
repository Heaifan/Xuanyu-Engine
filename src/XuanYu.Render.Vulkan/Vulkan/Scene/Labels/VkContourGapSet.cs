using System.Numerics;
using Silk.NET.Vulkan;
using XuanYu.Render;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkContourGapSet : IDisposable
{
    private readonly Vk _vk;
    private readonly PhysicalDevice _gpu;
    private readonly Device _device;
    private DescriptorPool _pool;
    private VkBuffer _buffer;
    private DeviceMemory _memory;
    private int _capacity;
    private readonly Dictionary<ReadOnlyMemory<Vector4>, (uint First, uint Count)> _ranges = [];
    public DescriptorSetLayout Layout { get; private set; }
    public DescriptorSet Set { get; private set; }

    public VkContourGapSet(Vk vk, PhysicalDevice gpu, Device device)
    {
        _vk = vk; _gpu = gpu; _device = device;
        try
        {
            var binding = new DescriptorSetLayoutBinding
            {
                Binding = 0, DescriptorCount = 1,
                DescriptorType = DescriptorType.StorageBuffer,
                StageFlags = ShaderStageFlags.FragmentBit
            };
            var layoutInfo = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1, PBindings = &binding
            };
            Check(vk.CreateDescriptorSetLayout(device, &layoutInfo, null, out var layout));
            Layout = layout;
            var size = new DescriptorPoolSize { Type = DescriptorType.StorageBuffer,
                DescriptorCount = 1 };
            var poolInfo = new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                MaxSets = 1, PoolSizeCount = 1, PPoolSizes = &size
            };
            Check(vk.CreateDescriptorPool(device, &poolInfo, null, out _pool));
            DescriptorSetLayout rawLayout = Layout;
            var alloc = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = _pool, DescriptorSetCount = 1,
                PSetLayouts = &rawLayout
            };
            DescriptorSet set;
            Check(vk.AllocateDescriptorSets(device, &alloc, &set));
            Set = set;
            EnsureCapacity(256);
        }
        catch { Dispose(); throw; }
    }

    private static void Check(Result result)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"等高线标高 GPU 缓冲失败：{result}");
    }

    public void Dispose()
    {
        DestroyStorage();
        if (_pool.Handle != 0) _vk.DestroyDescriptorPool(_device, _pool, null);
        if (Layout.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, Layout, null);
        _pool = default; Layout = default;
    }
}