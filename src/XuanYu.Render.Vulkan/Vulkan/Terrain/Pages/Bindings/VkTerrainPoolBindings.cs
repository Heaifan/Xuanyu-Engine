using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainPoolBindings : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly VkTerrainBuffer _uniform;
    private readonly DescriptorSet[] _sets;
    private readonly byte[] _parameters;
    private readonly int _stride;
    private DescriptorPool _pool;

    public VkTerrainPoolBindings(Vk vk, PhysicalDevice gpu, Device device,
        DescriptorSetLayout layout, VkTerrainPageArray image, int capacity, VkDemTexture water)
    {
        _vk = vk; _device = device;
        vk.GetPhysicalDeviceProperties(gpu, out var props);
        ulong align = Math.Max(1UL, props.Limits.MinUniformBufferOffsetAlignment);
        _stride = checked((int)(((ulong)VkTerrainDrawParameters.ByteSize + align - 1) / align * align));
        _parameters = new byte[checked(capacity * _stride)];
        _sets = new DescriptorSet[capacity];
        _uniform = new VkTerrainBuffer(vk, gpu, device, (ulong)_parameters.Length, BufferUsageFlags.UniformBufferBit);
        try { CreateSets(layout, image, water); }
        catch { Dispose(); throw; }
    }

    public void SetParameters(int slot, ReadOnlySpan<byte> parameters) =>
        parameters.CopyTo(_parameters.AsSpan(slot * _stride, VkTerrainDrawParameters.ByteSize));

    public void FlushParameters() => _uniform.Write(_parameters);

    public void Bind(CommandBuffer command, PipelineLayout layout, int slot)
    {
        var set = _sets[slot];
        _vk.CmdBindDescriptorSets(command, PipelineBindPoint.Graphics, layout, 0, 1, &set, 0, null);
    }

    public void Dispose()
    {
        if (_pool.Handle != 0) _vk.DestroyDescriptorPool(_device, _pool, null);
        _pool = default;
        _uniform.Dispose();
    }
}