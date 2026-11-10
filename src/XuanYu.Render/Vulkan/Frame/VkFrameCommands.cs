
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal static unsafe class VkFrameCommands
{
    public static void Record(Vk vk, CommandBuffer command, Image image)
    {
        Check(vk.ResetCommandBuffer(command, 0), "重置命令缓冲区");
        var begin = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        Check(vk.BeginCommandBuffer(command, &begin), "开始命令缓冲区");
        var range = new ImageSubresourceRange
        {
            AspectMask = ImageAspectFlags.ColorBit,
            LevelCount = 1,
            LayerCount = 1
        };
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            SrcQueueFamilyIndex = uint.MaxValue,
            DstQueueFamilyIndex = uint.MaxValue,
            OldLayout = ImageLayout.Undefined,
            NewLayout = ImageLayout.TransferDstOptimal,
            DstAccessMask = AccessFlags.TransferWriteBit,
            Image = image,
            SubresourceRange = range
        };
        vk.CmdPipelineBarrier(command,
            PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.TransferBit,
            0, 0, null, 0, null, 1, &barrier);
        var color = new ClearColorValue
        {
            Float32_0 = 0.035f,
            Float32_1 = 0.105f,
            Float32_2 = 0.220f,
            Float32_3 = 1.0f
        };
        vk.CmdClearColorImage(command, image,
            ImageLayout.TransferDstOptimal, &color, 1, &range);
        barrier.OldLayout = ImageLayout.TransferDstOptimal;
        barrier.NewLayout = ImageLayout.PresentSrcKhr;
        barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
        barrier.DstAccessMask = 0;
        vk.CmdPipelineBarrier(command,
            PipelineStageFlags.TransferBit, PipelineStageFlags.BottomOfPipeBit,
            0, 0, null, 0, null, 1, &barrier);
        Check(vk.EndCommandBuffer(command), "结束命令缓冲区");
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }
}
