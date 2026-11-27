using Silk.NET.Vulkan;
using System.Diagnostics;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkSharedColorProbe
{
    private VkMeshScene? _scene;

    public string DrawScene(IReadOnlyList<RenderDraw> draws, uint width, uint height)
    {
        bool profile = Environment.GetEnvironmentVariable("XYE_G06_PERF") == "1";
        long total = profile ? Stopwatch.GetTimestamp() : 0;
        _scene ??= new VkMeshScene(_vk, _gpu, _device,
            Format.R8G8B8A8Unorm, new Extent2D(width, height),
            new[] { _image }, true);
        long stage = profile ? Stopwatch.GetTimestamp() : 0;
        long creates = _scene.VertexBufferCreateCount, bytes = _scene.VertexBufferUploadBytes;
        _scene.SyncMeshes(draws);
        _scene.SyncDem(draws, _queue, _family);
        _scene.SyncWater(draws, _queue, _family);
        long synced = profile ? Stopwatch.GetTimestamp() : 0;
        VkFrameCommands.Record(_vk, _command, _scene, 0,
            new Extent2D(width, height), draws);
        long recorded = profile ? Stopwatch.GetTimestamp() : 0;
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
        long submitted = profile ? Stopwatch.GetTimestamp() : 0;
        Check(_vk.WaitForFences(_device, 1, &fence, true, ulong.MaxValue),
            "等待共享 3D 网格完成");
        if (!profile) return string.Empty;
        return $"Sync={Stopwatch.GetElapsedTime(stage, synced).TotalMilliseconds:F1}ms " +
            $"命令={Stopwatch.GetElapsedTime(synced, recorded).TotalMilliseconds:F1}ms " +
            $"Submit={Stopwatch.GetElapsedTime(recorded, submitted).TotalMilliseconds:F1}ms " +
            $"Fence完成等待={Stopwatch.GetElapsedTime(submitted).TotalMilliseconds:F1}ms " +
            $"Buffer新建={_scene.VertexBufferCreateCount - creates} " +
            $"顶点上传={_scene.VertexBufferUploadBytes - bytes}字节 " +
            $"Vulkan段总计={Stopwatch.GetElapsedTime(total).TotalMilliseconds:F1}ms";
    }
}
