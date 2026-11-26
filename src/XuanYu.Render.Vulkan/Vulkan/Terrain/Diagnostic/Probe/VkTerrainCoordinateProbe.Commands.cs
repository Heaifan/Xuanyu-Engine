using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainCoordinateProbe
{
    private void CreateCommands()
    {
        var pool = new CommandPoolCreateInfo { SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = _family, Flags = CommandPoolCreateFlags.TransientBit };
        Check(_vk.CreateCommandPool(_device, &pool, null, out _commandPool), "创建坐标诊断命令池");
        var alloc = new CommandBufferAllocateInfo { SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool, Level = CommandBufferLevel.Primary, CommandBufferCount = 1 };
        Check(_vk.AllocateCommandBuffers(_device, &alloc, out _command), "分配坐标诊断命令缓冲区");
        var fence = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
        Check(_vk.CreateFence(_device, &fence, null, out _fence), "创建坐标诊断 Fence");
    }

    private void Dispatch()
    {
        var begin = new CommandBufferBeginInfo { SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit };
        Check(_vk.BeginCommandBuffer(_command, &begin), "开始坐标诊断命令");
        _vk.CmdBindPipeline(_command, PipelineBindPoint.Compute, _pipeline);
        DescriptorSet set = _set;
        _vk.CmdBindDescriptorSets(_command, PipelineBindPoint.Compute, _pipelineLayout,
            0, 1, &set, 0, null);
        _vk.CmdDispatch(_command, 18, 1, 1);
        var barrier = new MemoryBarrier { SType = StructureType.MemoryBarrier,
            SrcAccessMask = AccessFlags.ShaderWriteBit, DstAccessMask = AccessFlags.HostReadBit };
        _vk.CmdPipelineBarrier(_command, PipelineStageFlags.ComputeShaderBit,
            PipelineStageFlags.HostBit, 0, 1, &barrier, 0, null, 0, null);
        Check(_vk.EndCommandBuffer(_command), "结束坐标诊断命令");
        CommandBuffer command = _command;
        var submit = new SubmitInfo { SType = StructureType.SubmitInfo,
            CommandBufferCount = 1, PCommandBuffers = &command };
        Fence fence = _fence;
        Check(_vk.QueueSubmit(_queue, 1, &submit, fence), "提交坐标诊断计算");
        _submitted = true;
        Check(_vk.WaitForFences(_device, 1, &fence, true, ulong.MaxValue), "等待坐标诊断读回");
        _submitted = false;
    }
}
