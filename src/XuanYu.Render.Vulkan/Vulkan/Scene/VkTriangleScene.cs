
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed class VkTriangleScene : IDisposable
{
    private readonly VkTrianglePass _pass;
    private readonly VkTriangleTargets _targets;
    private readonly VkTrianglePipeline _pipeline;
    private readonly VkTriangleVertexBuffer _vertices;

    public RenderPass RenderPass => _pass.Handle;
    public Framebuffer GetFramebuffer(uint index) => _targets[index];
    public Pipeline Pipeline => _pipeline.Handle;
    public PipelineLayout Layout => _pipeline.Layout;
    public Silk.NET.Vulkan.Buffer VertexBuffer => _vertices.Handle;

    public VkTriangleScene(
        Vk vk, PhysicalDevice gpu, Device device, VkSwapchain chain)
    {
        VkTrianglePass? pass = null;
        VkTriangleTargets? targets = null;
        VkTrianglePipeline? pipeline = null;
        VkTriangleVertexBuffer? vertices = null;

        try
        {
            pass = new VkTrianglePass(vk, device, chain.Format);
            targets = new VkTriangleTargets(
                vk, device, chain, pass.Handle);
            pipeline = new VkTrianglePipeline(
                vk, device, pass.Handle, chain.Extent);
            vertices = new VkTriangleVertexBuffer(vk, gpu, device);

            _pass = pass;
            _targets = targets;
            _pipeline = pipeline;
            _vertices = vertices;
        }
        catch
        {
            vertices?.Dispose();
            pipeline?.Dispose();
            targets?.Dispose();
            pass?.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _vertices.Dispose();
        _pipeline.Dispose();
        _targets.Dispose();
        _pass.Dispose();
    }
}
