using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkSharedColorProbe
{
    private void CreateCommands(uint family)
    {
        var poolInfo = new CommandPoolCreateInfo
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = family,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };
        Check(_vk.CreateCommandPool(_device, &poolInfo, null, out _pool), "命令池");
        var cmdInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _pool, Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };
        Check(_vk.AllocateCommandBuffers(_device, &cmdInfo, out _command), "命令缓冲区");
        var fenceInfo = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
        Check(_vk.CreateFence(_device, &fenceInfo, null, out _fence), "GPU Fence");
    }

    public void DrawColor()
    {
        var begin = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        Check(_vk.BeginCommandBuffer(_command, &begin), "开始 Vulkan 绘制");
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.Undefined, NewLayout = ImageLayout.General,
            SrcQueueFamilyIndex = uint.MaxValue, DstQueueFamilyIndex = uint.MaxValue,
            Image = _image, DstAccessMask = AccessFlags.TransferWriteBit,
            SubresourceRange = new ImageSubresourceRange
                { AspectMask = ImageAspectFlags.ColorBit, LevelCount = 1, LayerCount = 1 }
        };
        _vk.CmdPipelineBarrier(_command, PipelineStageFlags.TopOfPipeBit,
            PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier);
        var color = new ClearColorValue
            { Float32_0 = 0.08f, Float32_1 = 0.43f,
              Float32_2 = 0.73f, Float32_3 = 1f };
        var range = barrier.SubresourceRange;
        _vk.CmdClearColorImage(_command, _image, ImageLayout.General, &color, 1, &range);
        barrier.OldLayout = ImageLayout.General;
        barrier.NewLayout = ImageLayout.General;
        barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
        barrier.DstAccessMask = AccessFlags.MemoryReadBit;
        _vk.CmdPipelineBarrier(_command, PipelineStageFlags.TransferBit,
            PipelineStageFlags.AllCommandsBit, 0, 0, null, 0, null, 1, &barrier);
        Check(_vk.EndCommandBuffer(_command), "提交 Vulkan 命令");
        ulong acquireKey = 0, releaseKey = 1;
        uint timeout = 1000;
        DeviceMemory memory = _memory;
        var mutex = new Win32KeyedMutexAcquireReleaseInfoKHR
        {
            SType = StructureType.Win32KeyedMutexAcquireReleaseInfoKhr,
            AcquireCount = 1, PAcquireSyncs = &memory,
            PAcquireKeys = &acquireKey, PAcquireTimeouts = &timeout,
            ReleaseCount = 1, PReleaseSyncs = &memory, PReleaseKeys = &releaseKey
        };
        CommandBuffer command = _command;
        var submit = new SubmitInfo
        {
            SType = StructureType.SubmitInfo, PNext = &mutex,
            CommandBufferCount = 1, PCommandBuffers = &command
        };
        Check(_vk.QueueSubmit(_queue, 1, &submit, _fence), "Vulkan KeyedMutex 提交");
        Fence fence = _fence;
        Check(_vk.WaitForFences(_device, 1, &fence, true, ulong.MaxValue),
            "等待 Vulkan 绘制完成");
    }

    public void Dispose()
    {
        _vk.DeviceWaitIdle(_device);
        _scene?.Dispose();
        _scene = null;
        if (_fence.Handle != 0) _vk.DestroyFence(_device, _fence, null);
        if (_pool.Handle != 0) _vk.DestroyCommandPool(_device, _pool, null);
        if (_image.Handle != 0) _vk.DestroyImage(_device, _image, null);
        if (_memory.Handle != 0) _vk.FreeMemory(_device, _memory, null);
        _fence = default; _pool = default; _image = default; _memory = default;
    }
}