using Silk.NET.Vulkan;
using XuanYu.Render;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed partial class VkMeshScene : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly PhysicalDevice _gpu;
    private readonly VkTrianglePass _pass;
    private readonly VkTriangleTargets _targets;
    private readonly VkTrianglePipeline _pipeline;
    private readonly VkDemDescriptors _descriptors;
    private readonly Dictionary<RenderMesh, VkMeshBuffer> _buffers = [];
    public RenderPass RenderPass => _pass.Handle;
    public Pipeline Pipeline => _pipeline.Handle;
    public Pipeline SelectionMaskPipeline => _pipeline.Mask;
    public Pipeline SelectionOutlinePipeline => _pipeline.Outline;
    public Pipeline OverlayMaskPipeline => _pipeline.OverlayMask;
    public Pipeline OverlayOutlinePipeline => _pipeline.OverlayOutline;
    public PipelineLayout Layout => _pipeline.Layout;
    public Framebuffer GetFramebuffer(uint index) => _targets[index];
    public VkBuffer BufferFor(RenderMesh mesh) => _buffers[mesh].Handle;

    public VkMeshScene(Vk vk, PhysicalDevice gpu, Device device, VkSwapchain chain)
        : this(vk, gpu, device, chain.Format, chain.Extent, chain.Images, false) { }

    public VkMeshScene(Vk vk, PhysicalDevice gpu, Device device,
        Format format, Extent2D extent, Image[] images, bool offscreen)
    {
        _vk = vk;
        _gpu = gpu;
        _device = device;
        VkMsaaSettings.Validate(vk, gpu);
        VkTrianglePass? pass = null;
        VkTriangleTargets? targets = null;
        VkTrianglePipeline? pipeline = null;
        VkDemDescriptors? descriptors = null;
        try
        {
            descriptors = new VkDemDescriptors(vk, device);
            pass = new VkTrianglePass(vk, device, format, offscreen);
            targets = new VkTriangleTargets(vk, gpu, device, extent, format, images, pass.Handle);
            pipeline = new VkTrianglePipeline(vk, device, pass.Handle, extent, descriptors.Layout);
            _pass = pass;
            _targets = targets;
            _pipeline = pipeline;
            _descriptors = descriptors;
        }
        catch
        {
            pipeline?.Dispose();
            targets?.Dispose();
            pass?.Dispose();
            descriptors?.Dispose();
            throw;
        }
    }

    public void SyncMeshes(IReadOnlyList<RenderDraw> draws)
    {
        HashSet<RenderMesh> active = new(draws.Select(x => x.Mesh));
        foreach (RenderMesh old in _buffers.Keys.ToArray())
            if (!active.Contains(old))
            {
                _buffers[old].Dispose();
                _buffers.Remove(old);
            }
        foreach (RenderDraw draw in draws)
        {
            if (!draw.Visible) continue;
            if (_buffers.TryGetValue(draw.Mesh, out var old))
            {
                if (old.Matches(draw.DemUV)) continue;
                old.Dispose();
                _buffers.Remove(draw.Mesh);
            }
            _buffers.Add(draw.Mesh,
                new VkMeshBuffer(_vk, _gpu, _device, draw.Mesh, draw.DemUV));
        }
    }

    public void Dispose()
    {
        DisposeDem();
        foreach (VkMeshBuffer buffer in _buffers.Values) buffer.Dispose();
        _buffers.Clear();
        _pipeline.Dispose();
        _descriptors.Dispose();
        _targets.Dispose();
        _pass.Dispose();
    }
}