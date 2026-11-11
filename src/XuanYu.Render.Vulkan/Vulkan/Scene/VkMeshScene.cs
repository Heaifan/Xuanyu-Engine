
using Silk.NET.Vulkan;
using XuanYu.Render;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed class VkMeshScene : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly PhysicalDevice _gpu;
    private readonly VkTrianglePass _pass;
    private readonly VkTriangleTargets _targets;
    private readonly VkTrianglePipeline _pipeline;
    private readonly Dictionary<RenderMesh, VkMeshBuffer> _buffers = [];

    public RenderPass RenderPass => _pass.Handle;
    public Pipeline Pipeline => _pipeline.Handle;
    public PipelineLayout Layout => _pipeline.Layout;
    public Framebuffer GetFramebuffer(uint index) => _targets[index];
    public VkBuffer BufferFor(RenderMesh mesh) => _buffers[mesh].Handle;

    public VkMeshScene(
        Vk vk, PhysicalDevice gpu, Device device, VkSwapchain chain)
    {
        _vk = vk;
        _gpu = gpu;
        _device = device;

        VkTrianglePass? pass = null;
        VkTriangleTargets? targets = null;
        VkTrianglePipeline? pipeline = null;

        try
        {
            pass = new VkTrianglePass(vk, device, chain.Format);
            targets = new VkTriangleTargets(
                vk, device, chain, pass.Handle);
            pipeline = new VkTrianglePipeline(
                vk, device, pass.Handle, chain.Extent);

            _pass = pass;
            _targets = targets;
            _pipeline = pipeline;
        }
        catch
        {
            pipeline?.Dispose();
            targets?.Dispose();
            pass?.Dispose();
            throw;
        }
    }

    public void SyncMeshes(IReadOnlyList<RenderDraw> draws)
    {
        HashSet<RenderMesh> active =
            new(draws.Select(x => x.Mesh));

        foreach (RenderMesh old in _buffers.Keys.ToArray())
            if (!active.Contains(old))
            {
                _buffers[old].Dispose();
                _buffers.Remove(old);
            }

        foreach (RenderDraw draw in draws)
            if (draw.Visible && !_buffers.ContainsKey(draw.Mesh))
                _buffers.Add(draw.Mesh,
                    new VkMeshBuffer(_vk, _gpu, _device, draw.Mesh));
    }

    public void Dispose()
    {
        foreach (VkMeshBuffer buffer in _buffers.Values)
            buffer.Dispose();

        _buffers.Clear();
        _pipeline.Dispose();
        _targets.Dispose();
        _pass.Dispose();
    }
}
