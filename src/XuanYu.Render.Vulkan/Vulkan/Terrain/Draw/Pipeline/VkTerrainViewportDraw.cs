using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed class VkTerrainViewportDraw : IDisposable
{
    private readonly Vk _vk;
    private readonly VkTerrainDrawPipeline _pipeline;
    private readonly VkTerrainTopology _topology;
    public VkTerrainPagePool Pool { get; }

    public VkTerrainViewportDraw(Vk vk, PhysicalDevice gpu, Device device,
        Queue queue, uint family, VkMeshScene scene, Extent2D extent,
        int capacity, int uploadLimit, ulong budget, IReadOnlyList<int[]> topology, RenderWaterAtlas? water = null)
    {
        _vk = vk;
        _pipeline = new VkTerrainDrawPipeline(vk, device, scene.RenderPass, extent);
        try
        {
            _topology = new VkTerrainTopology(vk, gpu, device, topology);
            try
            {
                Pool = new VkTerrainPagePool(vk, gpu, device, queue, family,
                    _pipeline.SetLayout, capacity, uploadLimit, budget, water);
            }
            catch { _topology.Dispose(); throw; }
        }
        catch { _pipeline.Dispose(); throw; }
    }

    // UBO 写入必须发生在帧命令记录之前，且上一帧已经完成。
    public Action<CommandBuffer> Prepare(IReadOnlyList<TerrainPoolDraw> draws)
    {
        var prepared = Pool.PrepareDraws(draws);
        return command =>
        {
            _vk.CmdBindPipeline(command, PipelineBindPoint.Graphics, _pipeline.Handle);
            Pool.Draw(_vk, command, _pipeline.Layout, _topology, prepared);
        };
    }

    public void Dispose()
    {
        try { Pool.Dispose(); }
        finally { _topology.Dispose(); _pipeline.Dispose(); }
    }
}