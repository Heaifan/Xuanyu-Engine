using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainCoordinateProbe : IDisposable
{
    private readonly Vk _vk;
    private readonly PhysicalDevice _physical;
    private readonly Device _device;
    private readonly Queue _queue;
    private readonly uint _family;
    private VkBuffer _input, _output;
    private DeviceMemory _inputMemory, _outputMemory;
    private DescriptorSetLayout _layout;
    private DescriptorPool _descriptorPool;
    private DescriptorSet _set;
    private PipelineLayout _pipelineLayout;
    private Pipeline _pipeline;
    private CommandPool _commandPool;
    private CommandBuffer _command;
    private Fence _fence;
    private bool _submitted;

    public VkTerrainCoordinateProbe(Vk vk, PhysicalDevice physical, Device device,
        Queue queue, uint family)
    {
        _vk = vk; _physical = physical; _device = device; _queue = queue; _family = family;
        try { CreateBuffers(); CreatePipeline(); CreateCommands(); }
        catch { Dispose(); throw; }
    }
}
