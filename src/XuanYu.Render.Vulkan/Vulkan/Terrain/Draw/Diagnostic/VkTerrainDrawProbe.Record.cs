using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainDrawProbe
{
    private void Record(RenderPass pass, Framebuffer framebuffer, Extent2D extent,
        Image image, VkTerrainBuffer readback, VkTerrainDrawPipeline pipeline,
        Action<CommandBuffer> draw)
    {
        Check(_vk.ResetCommandBuffer(_command, 0), "重置 G07 绘制命令");
        var begin = new CommandBufferBeginInfo { SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit };
        Check(_vk.BeginCommandBuffer(_command, &begin), "开始 G07 绘制命令");
        ClearValue* clears = stackalloc ClearValue[2];
        clears[0] = new ClearValue { Color = new ClearColorValue { Float32_3 = 1f } };
        clears[1] = new ClearValue { DepthStencil = new ClearDepthStencilValue { Depth = 1f } };
        var render = new RenderPassBeginInfo { SType = StructureType.RenderPassBeginInfo,
            RenderPass = pass, Framebuffer = framebuffer, RenderArea = new Rect2D(new Offset2D(0, 0), extent),
            ClearValueCount = 2, PClearValues = clears };
        _vk.CmdBeginRenderPass(_command, &render, SubpassContents.Inline);
        _vk.CmdBindPipeline(_command, PipelineBindPoint.Graphics, pipeline.Handle);
        draw(_command);
        _vk.CmdEndRenderPass(_command);
        CopyToHost(image, extent, readback);
        Check(_vk.EndCommandBuffer(_command), "结束 G07 绘制命令");
    }

    private void CopyToHost(Image image, Extent2D extent, VkTerrainBuffer readback)
    {
        var barrier = new ImageMemoryBarrier { SType = StructureType.ImageMemoryBarrier,
            Image = image, OldLayout = ImageLayout.General, NewLayout = ImageLayout.TransferSrcOptimal,
            SrcQueueFamilyIndex = uint.MaxValue, DstQueueFamilyIndex = uint.MaxValue,
            SrcAccessMask = AccessFlags.ColorAttachmentWriteBit, DstAccessMask = AccessFlags.TransferReadBit,
            SubresourceRange = new ImageSubresourceRange { AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = 1, LayerCount = 1 } };
        _vk.CmdPipelineBarrier(_command, PipelineStageFlags.ColorAttachmentOutputBit,
            PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier);
        var region = new BufferImageCopy { ImageSubresource = new ImageSubresourceLayers
            { AspectMask = ImageAspectFlags.ColorBit, LayerCount = 1 },
            ImageExtent = new Extent3D(extent.Width, extent.Height, 1) };
        _vk.CmdCopyImageToBuffer(_command, image, ImageLayout.TransferSrcOptimal, readback.Handle, 1, &region);
        var host = new MemoryBarrier { SType = StructureType.MemoryBarrier,
            SrcAccessMask = AccessFlags.TransferWriteBit, DstAccessMask = AccessFlags.HostReadBit };
        _vk.CmdPipelineBarrier(_command, PipelineStageFlags.TransferBit, PipelineStageFlags.HostBit,
            0, 1, &host, 0, null, 0, null);
    }
}