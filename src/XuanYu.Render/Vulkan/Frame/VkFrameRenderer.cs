
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkFrameRenderer : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly Queue _queue;
    private readonly VkSwapchain _swapchain;
    private readonly VkFrameResources _resources;

    public VkFrameRenderer(Vk vk, Device device, Queue queue,
        uint family, VkSwapchain swapchain)
    {
        _vk = vk;
        _device = device;
        _queue = queue;
        _swapchain = swapchain;
        _resources = new VkFrameResources(
            vk, device, family, swapchain.Images.Length);
    }

    public Result PresentFrame()
    {
        Fence fence = _resources.Fence;
        Result wait = _vk.WaitForFences(_device, 1, &fence, true, 0);

        if (wait == Result.Timeout)
            return Result.NotReady;

        Check(wait, "等待上一帧");

        uint index = 0;
        Result acquired = _swapchain.Api.AcquireNextImage(
            _device, _swapchain.Handle, 0, _resources.Available,
            default, ref index);

        if (acquired == Result.NotReady || acquired == Result.Timeout ||
            acquired == Result.ErrorOutOfDateKhr)
            return acquired;

        if (acquired != Result.Success && acquired != Result.SuboptimalKhr)
            throw new InvalidOperationException(
                $"获取交换链图像失败：{acquired}");

        CommandBuffer command = _resources.Command;
        VkFrameCommands.Record(_vk, command, _swapchain.Images[index]);

        var available = _resources.Available;
        var finished = _resources.Finished(index);
        var stage = PipelineStageFlags.TransferBit;

        var submit = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &available,
            PWaitDstStageMask = &stage,
            CommandBufferCount = 1,
            PCommandBuffers = &command,
            SignalSemaphoreCount = 1,
            PSignalSemaphores = &finished
        };

        Check(_vk.ResetFences(_device, 1, &fence), "重置帧 Fence");
        Check(_vk.QueueSubmit(_queue, 1, &submit, fence), "提交 GPU 清屏");

        SwapchainKHR swapchain = _swapchain.Handle;

        var present = new PresentInfoKHR
        {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &finished,
            SwapchainCount = 1,
            PSwapchains = &swapchain,
            PImageIndices = &index
        };

        Result result = _swapchain.Api.QueuePresent(_queue, &present);

        if (result != Result.Success &&
            result != Result.SuboptimalKhr &&
            result != Result.ErrorOutOfDateKhr)
            throw new InvalidOperationException(
                $"Vulkan 呈现失败：{result}");

        return result;
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException(
                $"{action}失败：{result}");
    }

    public void Dispose() => _resources.Dispose();
}
