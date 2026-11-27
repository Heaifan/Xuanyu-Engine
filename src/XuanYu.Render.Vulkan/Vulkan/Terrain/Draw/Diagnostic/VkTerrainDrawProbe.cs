using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainDrawProbe : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly Queue _queue;
    private CommandPool _pool;
    private CommandBuffer _command;
    private Fence _fence;
    private bool _submitted;

    public VkTerrainDrawProbe(Vk vk, Device device, Queue queue, uint family)
    {
        _vk = vk; _device = device; _queue = queue;
        try
        {
            var pool = new CommandPoolCreateInfo { SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = family, Flags = CommandPoolCreateFlags.ResetCommandBufferBit };
            Check(vk.CreateCommandPool(device, &pool, null, out _pool), "创建 G07 绘制诊断命令池");
            var alloc = new CommandBufferAllocateInfo { SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = _pool, Level = CommandBufferLevel.Primary, CommandBufferCount = 1 };
            Check(vk.AllocateCommandBuffers(device, &alloc, out _command), "分配 G07 绘制命令");
            var fence = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
            Check(vk.CreateFence(device, &fence, null, out _fence), "创建 G07 绘制 Fence");
        }
        catch { Dispose(); throw; }
    }

    public byte[] Draw(RenderPass pass, Framebuffer framebuffer, Extent2D extent,
        Image image, VkTerrainBuffer readback, VkTerrainDrawPipeline pipeline,
        VkTerrainDrawPage page, VkTerrainTopology topology, int mask)
        => DrawCommands(pass, framebuffer, extent, image, readback, pipeline, command =>
        {
            page.Bind(command, pipeline.Layout);
            topology.Draw(_vk, command, mask);
        });

    public byte[] DrawCommands(RenderPass pass, Framebuffer framebuffer, Extent2D extent,
        Image image, VkTerrainBuffer readback, VkTerrainDrawPipeline pipeline, Action<CommandBuffer> draw)
    {
        Record(pass, framebuffer, extent, image, readback, pipeline, draw);
        SubmitAndWait();
        return readback.Read();
    }

    private void SubmitAndWait()
    {
        var command = _command;
        var fence = _fence;
        var submit = new SubmitInfo { SType = StructureType.SubmitInfo,
            CommandBufferCount = 1, PCommandBuffers = &command };
        Check(_vk.ResetFences(_device, 1, &fence), "重置 G07 绘制 Fence");
        Check(_vk.QueueSubmit(_queue, 1, &submit, fence), "提交 G07 绘制");
        _submitted = true;
        Check(_vk.WaitForFences(_device, 1, &fence, true, ulong.MaxValue), "等待 G07 绘制完成");
        _submitted = false;
    }

    public void Dispose()
    {
        if (_submitted) { var fence = _fence; _vk.WaitForFences(_device, 1, &fence, true, ulong.MaxValue); }
        if (_fence.Handle != 0) _vk.DestroyFence(_device, _fence, null);
        if (_pool.Handle != 0) _vk.DestroyCommandPool(_device, _pool, null);
        _fence = default; _pool = default;
    }
}