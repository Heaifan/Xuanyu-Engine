using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainPageTransfer : IDisposable
{
    // 2178 bytes of R16 data; each transfer offset is aligned to 4 bytes.
    internal const int PageStride = 2180;
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly Queue _queue;
    private readonly VkTerrainBuffer _staging;
    private readonly byte[] _scratch;
    private CommandPool _pool;
    private CommandBuffer _command;
    private Fence _fence;
    private bool _submitted;

    public VkTerrainPageTransfer(Vk vk, PhysicalDevice gpu, Device device, Queue queue, uint family, int limit)
    {
        _vk = vk; _device = device; _queue = queue;
        _scratch = new byte[checked(limit * PageStride)];
        _staging = new VkTerrainBuffer(vk, gpu, device, (ulong)_scratch.Length, BufferUsageFlags.TransferSrcBit);
        try
        {
            var pool = new CommandPoolCreateInfo { SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = family, Flags = CommandPoolCreateFlags.ResetCommandBufferBit };
            Check(vk.CreateCommandPool(device, &pool, null, out _pool), "创建 G07 上传命令池");
            var alloc = new CommandBufferAllocateInfo { SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = _pool, Level = CommandBufferLevel.Primary, CommandBufferCount = 1 };
            Check(vk.AllocateCommandBuffers(device, &alloc, out _command), "分配 G07 上传命令");
            var fence = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
            Check(vk.CreateFence(device, &fence, null, out _fence), "创建 G07 上传 Fence");
        }
        catch { Dispose(); throw; }
    }

    public void Submit(Image image, (int Layer, bool Reused, ReadOnlyMemory<short> Heights)[] pages)
    {
        for (int i = 0; i < pages.Length; i++)
            MemoryMarshal.AsBytes(pages[i].Heights.Span).CopyTo(_scratch.AsSpan(i * PageStride));
        _staging.Write(_scratch.AsSpan(0, pages.Length * PageStride));
        Record(image, pages);
        var command = _command;
        var fence = _fence;
        Check(_vk.ResetFences(_device, 1, &fence), "重置 G07 上传 Fence");
        var submit = new SubmitInfo { SType = StructureType.SubmitInfo,
            CommandBufferCount = 1, PCommandBuffers = &command };
        Check(_vk.QueueSubmit(_queue, 1, &submit, fence), "提交 G07 高程批次");
        _submitted = true;
    }

    public bool Poll()
    {
        if (!_submitted) return true;
        var result = _vk.GetFenceStatus(_device, _fence);
        if (result == Result.NotReady) return false;
        Check(result, "查询 G07 上传 Fence");
        _submitted = false;
        return true;
    }

    public void Wait()
    {
        if (!_submitted) return;
        var fence = _fence;
        Check(_vk.WaitForFences(_device, 1, &fence, true, ulong.MaxValue), "等待 G07 高程批次");
        _submitted = false;
    }

    public void Dispose()
    {
        try { Wait(); }
        finally
        {
            if (_fence.Handle != 0) _vk.DestroyFence(_device, _fence, null);
            if (_pool.Handle != 0) _vk.DestroyCommandPool(_device, _pool, null);
            _fence = default; _pool = default;
            _staging.Dispose();
        }
    }
}