using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainPageTransfer
{
    private void Record(Image image, (int Layer, bool Reused, ReadOnlyMemory<short> Heights)[] pages)
    {
        Check(_vk.ResetCommandBuffer(_command, 0), "重置 G07 上传命令");
        var begin = new CommandBufferBeginInfo { SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit };
        Check(_vk.BeginCommandBuffer(_command, &begin), "开始 G07 批量上传");
        var barriers = new ImageMemoryBarrier[pages.Length];
        var copies = new BufferImageCopy[pages.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            var page = pages[i];
            barriers[i] = new ImageMemoryBarrier { SType = StructureType.ImageMemoryBarrier,
                Image = image, OldLayout = page.Reused ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.Undefined,
                NewLayout = ImageLayout.TransferDstOptimal, SrcQueueFamilyIndex = uint.MaxValue,
                DstQueueFamilyIndex = uint.MaxValue, SrcAccessMask = page.Reused ? AccessFlags.ShaderReadBit : 0,
                DstAccessMask = AccessFlags.TransferWriteBit,
                SubresourceRange = new ImageSubresourceRange { AspectMask = ImageAspectFlags.ColorBit,
                    BaseArrayLayer = (uint)page.Layer, LevelCount = 1, LayerCount = 1 } };
            copies[i] = new BufferImageCopy { BufferOffset = (ulong)(i * PageStride),
                ImageSubresource = new ImageSubresourceLayers { AspectMask = ImageAspectFlags.ColorBit,
                    BaseArrayLayer = (uint)page.Layer, LayerCount = 1 }, ImageExtent = new Extent3D(33, 33, 1) };
        }
        fixed (ImageMemoryBarrier* b = barriers)
        fixed (BufferImageCopy* c = copies)
        {
            _vk.CmdPipelineBarrier(_command, PipelineStageFlags.VertexShaderBit | PipelineStageFlags.TopOfPipeBit,
                PipelineStageFlags.TransferBit, 0, 0, null, 0, null, (uint)pages.Length, b);
            _vk.CmdCopyBufferToImage(_command, _staging.Handle, image, ImageLayout.TransferDstOptimal,
                (uint)pages.Length, c);
            for (int i = 0; i < barriers.Length; i++)
            {
                barriers[i].OldLayout = ImageLayout.TransferDstOptimal;
                barriers[i].NewLayout = ImageLayout.ShaderReadOnlyOptimal;
                barriers[i].SrcAccessMask = AccessFlags.TransferWriteBit;
                barriers[i].DstAccessMask = AccessFlags.ShaderReadBit;
            }
            _vk.CmdPipelineBarrier(_command, PipelineStageFlags.TransferBit, PipelineStageFlags.VertexShaderBit,
                0, 0, null, 0, null, (uint)pages.Length, b);
        }
        Check(_vk.EndCommandBuffer(_command), "结束 G07 批量上传");
    }
}