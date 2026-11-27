using Silk.NET.Vulkan;
using System.Diagnostics;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkSharedColorProbe
{
    private VkMeshScene? _scene;

    public string DrawScene(IReadOnlyList<RenderDraw> draws, uint width, uint height,
        IReadOnlyList<TerrainPoolDraw>? terrainDraws = null)
    {
        bool profile = Environment.GetEnvironmentVariable("XYE_G06_PERF") == "1";
        long total = profile ? Stopwatch.GetTimestamp() : 0;
        var scene = EnsureScene(width, height);
        long stage = profile ? Stopwatch.GetTimestamp() : 0;
        long creates = scene.VertexBufferCreateCount, bytes = scene.VertexBufferUploadBytes;
        scene.SyncMeshes(draws);
        scene.SyncDem(draws, _queue, _family);
        scene.SyncWater(draws, _queue, _family);
        long synced = profile ? Stopwatch.GetTimestamp() : 0;
        var terrain = terrainDraws is null ? null : (_terrain ??
            throw new InvalidOperationException("G07 页池尚未创建。")).Prepare(terrainDraws);
        VkFrameCommands.Record(_vk, _command, scene, 0,
            new Extent2D(width, height), draws, terrain);
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
            $"Buffer新建={scene.VertexBufferCreateCount - creates} " +
            $"顶点上传={scene.VertexBufferUploadBytes - bytes}字节 " +
            $"Vulkan段总计={Stopwatch.GetElapsedTime(total).TotalMilliseconds:F1}ms";
    }
}