using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTriangleTargets : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly VkDepthImage _depth;
    private readonly VkMsaaColorImage _color;
    private readonly ImageView[] _views;
    private readonly Framebuffer[] _buffers;
    public Framebuffer this[uint index] => _buffers[checked((int)index)];

    public VkTriangleTargets(Vk vk, PhysicalDevice gpu, Device device,
        VkSwapchain chain, RenderPass pass)
        : this(vk, gpu, device, chain.Extent, chain.Format, chain.Images, pass) { }

    public VkTriangleTargets(Vk vk, PhysicalDevice gpu, Device device,
        Extent2D extent, Format format, Image[] images, RenderPass pass)
    {
        _vk = vk;
        _device = device;
        _views = new ImageView[images.Length];
        _buffers = new Framebuffer[images.Length];
        _depth = new VkDepthImage(vk, gpu, device, extent);
        try { _color = new VkMsaaColorImage(vk, gpu, device, extent, format); }
        catch { _depth.Dispose(); throw; }
        try
        {
            ImageView* attachments = stackalloc ImageView[3];
            attachments[0] = _color.View;
            attachments[1] = _depth.View;
            for (int i = 0; i < _views.Length; i++)
            {
                var viewInfo = new ImageViewCreateInfo
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = images[i], ViewType = ImageViewType.Type2D,
                    Format = format,
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        LevelCount = 1, LayerCount = 1
                    }
                };
                Check(vk.CreateImageView(device, &viewInfo, null, out _views[i]),
                    "创建 Swapchain ImageView");
                attachments[2] = _views[i];
                var info = new FramebufferCreateInfo
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = pass, AttachmentCount = 3,
                    PAttachments = attachments,
                    Width = extent.Width, Height = extent.Height,
                    Layers = 1
                };
                Check(vk.CreateFramebuffer(device, &info, null, out _buffers[i]),
                    "创建 MSAA Framebuffer");
            }
        }
        catch { Dispose(); throw; }
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }

    public void Dispose()
    {
        foreach (Framebuffer buffer in _buffers)
            if (buffer.Handle != 0) _vk.DestroyFramebuffer(_device, buffer, null);
        foreach (ImageView view in _views)
            if (view.Handle != 0) _vk.DestroyImageView(_device, view, null);
        _color.Dispose();
        _depth.Dispose();
    }
}