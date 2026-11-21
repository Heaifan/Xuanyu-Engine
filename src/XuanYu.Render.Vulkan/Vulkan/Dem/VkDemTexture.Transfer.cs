using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkDemTexture
{
    private void Transfer(Queue queue, uint family, VkBuffer staging)
    {
        CommandPool pool = default;
        try
        {
            var poolInfo = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = family,
                Flags = CommandPoolCreateFlags.TransientBit
            };
            Check(_vk.CreateCommandPool(_device, &poolInfo, null, out pool),
                "创建 DEM 传输命令池");
            var alloc = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = pool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1
            };
            Check(_vk.AllocateCommandBuffers(_device, &alloc, out var cmd),
                "分配 DEM 传输命令");
            var begin = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit
            };
            Check(_vk.BeginCommandBuffer(cmd, &begin), "开始 DEM 纹理传输");
            var barrier = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                Image = _image,
                OldLayout = ImageLayout.Undefined,
                NewLayout = ImageLayout.TransferDstOptimal,
                SrcQueueFamilyIndex = uint.MaxValue,
                DstQueueFamilyIndex = uint.MaxValue,
                DstAccessMask = AccessFlags.TransferWriteBit,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    LevelCount = 1,
                    LayerCount = 1
                }
            };
            _vk.CmdPipelineBarrier(cmd, PipelineStageFlags.TopOfPipeBit,
                PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier);
            var region = new BufferImageCopy
            {
                ImageSubresource = new ImageSubresourceLayers
                    { AspectMask = ImageAspectFlags.ColorBit, LayerCount = 1 },
                ImageExtent = new Extent3D(3601, 3601, 1)
            };
            _vk.CmdCopyBufferToImage(cmd, staging, _image,
                ImageLayout.TransferDstOptimal, 1, &region);
            barrier.OldLayout = ImageLayout.TransferDstOptimal;
            barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;
            _vk.CmdPipelineBarrier(cmd, PipelineStageFlags.TransferBit,
                PipelineStageFlags.FragmentShaderBit, 0, 0, null, 0, null, 1, &barrier);
            Check(_vk.EndCommandBuffer(cmd), "结束 DEM 纹理传输");
            var submit = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &cmd
            };
            Check(_vk.QueueSubmit(queue, 1, &submit, default), "提交 DEM 纹理传输");
            // 传输阶段使用一次性命令池，等待完成后才释放 Staging。
            Check(_vk.QueueWaitIdle(queue), "等待 DEM 纹理传输完成");
        }
        finally
        {
            if (pool.Handle != 0)
                _vk.DestroyCommandPool(_device, pool, null);
        }
    }
}