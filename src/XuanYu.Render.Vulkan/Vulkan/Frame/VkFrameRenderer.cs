
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkFrameRenderer : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly Queue _queue;
    private readonly VkSwapchain _swapchain;
    private readonly VkFrameResources _resources;
    private readonly VkTriangleScene _scene;

    public VkFrameRenderer(Vk vk, PhysicalDevice gpu, Device device,
        Queue queue, uint family, VkSwapchain swapchain)
    {
        _vk = vk;
        _device = device;
        _queue = queue;
        _swapchain = swapchain;
        _scene = new VkTriangleScene(vk, gpu, device, swapchain);
        try
        {
            _resources = new VkFrameResources(
                vk, device, family, swapchain.Images.Length);
        }
        catch
        {
            _scene.Dispose();
            throw;
        }
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
            _device, _swapchain.Handle, 0,
            _resources.Available, default, ref index);
        if (acquired is Result.NotReady or Result.Timeout or Result.ErrorOutOfDateKhr)
            return acquired;
        if (acquired != Result.Success && acquired != Result.SuboptimalKhr)
            throw new InvalidOperationException($"获取交换链图像失败：{acquired}");
        CommandBuffer command = _resources.Command;
        VkFrameCommands.Record(_vk, command, _scene, index, _swapchain.Extent);
        var available = _resources.Available;
        var finished = _resources.Finished(index);
        var stage = PipelineStageFlags.ColorAttachmentOutputBit;
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
        Check(_vk.QueueSubmit(_queue, 1, &submit, fence), "提交三角形绘制");
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
            throw new InvalidOperationException($"Vulkan 呈现失败：{result}");
        return result;
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }

    public void Dispose()
    {
        _resources.Dispose();
        _scene.Dispose();
    }
}
