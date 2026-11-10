
using Silk.NET.Vulkan;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkFrameResources : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly VkSemaphore[] _finished;
    private CommandPool _pool;

    public CommandBuffer Command { get; private set; }
    public VkSemaphore Available { get; private set; }
    public Fence Fence { get; private set; }
    public VkSemaphore Finished(uint index) => _finished[index];

    public VkFrameResources(Vk vk, Device device, uint family, int imageCount)
    {
        _vk = vk;
        _device = device;
        _finished = new VkSemaphore[imageCount];
        try
        {
            var poolInfo = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = family,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };
            Check(vk.CreateCommandPool(device, &poolInfo, null, out _pool),
                "创建命令池");
            var allocateInfo = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = _pool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1
            };
            Check(vk.AllocateCommandBuffers(device, &allocateInfo, out var command),
                "分配命令缓冲区");
            Command = command;
            var semaphoreInfo = new SemaphoreCreateInfo
            {
                SType = StructureType.SemaphoreCreateInfo
            };
            Check(vk.CreateSemaphore(device, &semaphoreInfo, null, out var available),
                "创建图像可用信号量");
            Available = available;
            for (int i = 0; i < _finished.Length; i++)
            {
                Check(vk.CreateSemaphore(device, &semaphoreInfo, null,
                    out var finished), "创建呈现信号量");
                _finished[i] = finished;
            }
            var fenceInfo = new FenceCreateInfo
            {
                SType = StructureType.FenceCreateInfo,
                Flags = FenceCreateFlags.SignaledBit
            };
            Check(vk.CreateFence(device, &fenceInfo, null, out var fence),
                "创建帧 Fence");
            Fence = fence;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }

    public void Dispose()
    {
        foreach (var semaphore in _finished)
            if (semaphore.Handle != 0)
                _vk.DestroySemaphore(_device, semaphore, null);
        if (Available.Handle != 0)
            _vk.DestroySemaphore(_device, Available, null);
        if (Fence.Handle != 0)
            _vk.DestroyFence(_device, Fence, null);
        if (_pool.Handle != 0)
            _vk.DestroyCommandPool(_device, _pool, null);
        _pool = default;
    }
}
