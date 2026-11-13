using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTrianglePass : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    public RenderPass Handle { get; private set; }

    public VkTrianglePass(Vk vk, Device device, Format format, bool offscreen = false)
    {
        _vk = vk;
        _device = device;
        AttachmentDescription* attachments = stackalloc AttachmentDescription[3];
        attachments[0] = new AttachmentDescription
        {
            Format = format, Samples = VkMsaaSettings.Samples,
            LoadOp = AttachmentLoadOp.Clear, StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.ColorAttachmentOptimal
        };
        attachments[1] = new AttachmentDescription
        {
            Format = VkDepthImage.DepthFormat, Samples = VkMsaaSettings.Samples,
            LoadOp = AttachmentLoadOp.Clear, StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
        };
        attachments[2] = new AttachmentDescription
        {
            Format = format, Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.DontCare, StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = offscreen ? ImageLayout.General : ImageLayout.PresentSrcKhr
        };
        var color = new AttachmentReference
        {
            Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal
        };
        var depth = new AttachmentReference
        {
            Attachment = 1, Layout = ImageLayout.DepthStencilAttachmentOptimal
        };
        var resolve = new AttachmentReference
        {
            Attachment = 2, Layout = ImageLayout.ColorAttachmentOptimal
        };
        var subpass = new SubpassDescription
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1, PColorAttachments = &color,
            PDepthStencilAttachment = &depth, PResolveAttachments = &resolve
        };
        var dependency = new SubpassDependency
        {
            SrcSubpass = uint.MaxValue, DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit |
                PipelineStageFlags.LateFragmentTestsBit,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit |
                PipelineStageFlags.EarlyFragmentTestsBit,
            SrcAccessMask = AccessFlags.DepthStencilAttachmentWriteBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit |
                AccessFlags.DepthStencilAttachmentReadBit |
                AccessFlags.DepthStencilAttachmentWriteBit
        };
        var info = new RenderPassCreateInfo
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 3, PAttachments = attachments,
            SubpassCount = 1, PSubpasses = &subpass,
            DependencyCount = 1, PDependencies = &dependency
        };
        Result result = vk.CreateRenderPass(device, &info, null, out var pass);
        if (result != Result.Success)
            throw new InvalidOperationException($"创建 MSAA RenderPass 失败：{result}");
        Handle = pass;
    }

    public void Dispose()
    {
        if (Handle.Handle == 0) return;
        _vk.DestroyRenderPass(_device, Handle, null);
        Handle = default;
    }
}