using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkSharedColorProbe
{
    private VkMeshScene? _scene;

    public void DrawScene(IReadOnlyList<RenderDraw> draws, uint width, uint height)
    {
        _scene ??= new VkMeshScene(_vk, _gpu, _device,
            Format.R8G8B8A8Unorm, new Extent2D(width, height),
            new[] { _image }, true);
        _scene.SyncMeshes(draws);
        VkFrameCommands.Record(_vk, _command, _scene, 0,
            new Extent2D(width, height), draws);
        Fence fence = _fence;
        Check(_vk.ResetFences(_device, 1, &fence), "重置共享帧 Fence");
        DeviceMemory memory = _memory;
        ulong acquireKey = 0, releaseKey = 1;
        uint timeout = 1000;
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
        Check(_vk.QueueSubmit(_queue, 1, &submit, fence), "共享 3D 网格提交");
        Check(_vk.WaitForFences(_device, 1, &fence, true, ulong.MaxValue),
            "等待共享 3D 网格完成");
    }
}