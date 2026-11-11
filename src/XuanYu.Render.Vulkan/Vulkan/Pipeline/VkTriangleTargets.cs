
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTriangleTargets : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly ImageView[] _views;
    private readonly Framebuffer[] _buffers;

    public Framebuffer this[uint index] => _buffers[checked((int)index)];

    public VkTriangleTargets(Vk vk, Device device, VkSwapchain chain, RenderPass pass)
    {
        _vk = vk;
        _device = device;
        _views = new ImageView[chain.Images.Length];
        _buffers = new Framebuffer[chain.Images.Length];

        try
        {
            for (int i = 0; i < _views.Length; i++)
            {
                var viewInfo = new ImageViewCreateInfo
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = chain.Images[i],
                    ViewType = ImageViewType.Type2D,
                    Format = chain.Format,
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        LevelCount = 1,
                        LayerCount = 1
                    }
                };

                Check(vk.CreateImageView(device, &viewInfo, null, out _views[i]),
                    "创建 Swapchain ImageView");

                ImageView view = _views[i];
                var bufferInfo = new FramebufferCreateInfo
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = pass,
                    AttachmentCount = 1,
                    PAttachments = &view,
                    Width = chain.Extent.Width,
                    Height = chain.Extent.Height,
                    Layers = 1
                };

                Check(vk.CreateFramebuffer(device, &bufferInfo, null, out _buffers[i]),
                    "创建 Swapchain Framebuffer");
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }

    public void Dispose()
    {
        foreach (Framebuffer buffer in _buffers)
            if (buffer.Handle != 0)
                _vk.DestroyFramebuffer(_device, buffer, null);

        foreach (ImageView view in _views)
            if (view.Handle != 0)
                _vk.DestroyImageView(_device, view, null);
    }
}
