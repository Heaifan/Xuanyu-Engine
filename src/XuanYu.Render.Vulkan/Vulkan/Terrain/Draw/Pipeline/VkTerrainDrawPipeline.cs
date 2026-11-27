using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTerrainDrawPipeline : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    public DescriptorSetLayout SetLayout { get; private set; }
    public PipelineLayout Layout { get; private set; }
    public Pipeline Handle { get; private set; }

    public VkTerrainDrawPipeline(Vk vk, Device device, RenderPass pass, Extent2D extent)
    {
        _vk = vk; _device = device;
        try
        {
            DescriptorSetLayoutBinding* bindings = stackalloc DescriptorSetLayoutBinding[3];
            bindings[0] = new() { Binding = 0, DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler, StageFlags = ShaderStageFlags.VertexBit };
            bindings[1] = new() { Binding = 1, DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit };
            bindings[2] = new() { Binding = 2, DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler, StageFlags = ShaderStageFlags.FragmentBit };
            var setInfo = new DescriptorSetLayoutCreateInfo { SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 3, PBindings = bindings };
            Check(vk.CreateDescriptorSetLayout(device, &setInfo, null, out var set), "创建 G07 描述符布局");
            SetLayout = set;
            var layoutInfo = new PipelineLayoutCreateInfo { SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1, PSetLayouts = &set };
            Check(vk.CreatePipelineLayout(device, &layoutInfo, null, out var layout), "创建 G07 管线布局");
            Layout = layout;
            using var shaders = new VkTriangleShaders(vk, device, "g07.terrain.vert.spv", "g07.terrain.frag.spv");
            Handle = VkTrianglePipelineStates.Create(vk, device, pass, layout, extent,
                shaders.Vertex, shaders.Fragment, procedural: true);
        }
        catch { Dispose(); throw; }
    }

    public void Dispose()
    {
        if (Handle.Handle != 0) _vk.DestroyPipeline(_device, Handle, null);
        if (Layout.Handle != 0) _vk.DestroyPipelineLayout(_device, Layout, null);
        if (SetLayout.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, SetLayout, null);
        Handle = default; Layout = default; SetLayout = default;
    }
}