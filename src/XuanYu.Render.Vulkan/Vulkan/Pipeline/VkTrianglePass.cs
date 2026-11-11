
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTrianglePass : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;

    public RenderPass Handle { get; private set; }

    public VkTrianglePass(Vk vk, Device device, Format format)
    {
        _vk = vk;
        _device = device;

        var color = new AttachmentDescription
        {
            Format = format,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
        };

        var reference = new AttachmentReference
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal
        };

        var subpass = new SubpassDescription
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &reference
        };

        var dependency = new SubpassDependency
        {
            SrcSubpass = uint.MaxValue,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit
        };

        var info = new RenderPassCreateInfo
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &color,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency
        };

        Result result = vk.CreateRenderPass(
            device, &info, null, out var pass);

        if (result != Result.Success)
            throw new InvalidOperationException(
                $"创建三角形 RenderPass 失败：{result}");

        Handle = pass;
    }

    public void Dispose()
    {
        if (Handle.Handle == 0)
            return;

        _vk.DestroyRenderPass(_device, Handle, null);
        Handle = default;
    }
}
