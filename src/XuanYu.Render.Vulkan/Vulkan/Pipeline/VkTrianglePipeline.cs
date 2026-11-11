
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTrianglePipeline : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;

    public PipelineLayout Layout { get; private set; }
    public Pipeline Handle { get; private set; }

    public VkTrianglePipeline(Vk vk, Device device, RenderPass pass, Extent2D extent)
    {
        _vk = vk;
        _device = device;

        var range = new PushConstantRange
        {
            StageFlags = ShaderStageFlags.VertexBit,
            Offset = 0,
            Size = 80
        };
        var info = new PipelineLayoutCreateInfo
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &range
        };

        Result result = vk.CreatePipelineLayout(
            device, &info, null, out var layout);
        if (result != Result.Success)
            throw new InvalidOperationException(
                $"创建 PipelineLayout 失败：{result}");

        Layout = layout;
        try
        {
            using var shaders = new VkTriangleShaders(vk, device);
            Handle = VkTrianglePipelineStates.Create(
                vk, device, pass, layout, extent,
                shaders.Vertex, shaders.Fragment);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (Handle.Handle != 0)
        {
            _vk.DestroyPipeline(_device, Handle, null);
            Handle = default;
        }

        if (Layout.Handle != 0)
        {
            _vk.DestroyPipelineLayout(_device, Layout, null);
            Layout = default;
        }
    }
}
