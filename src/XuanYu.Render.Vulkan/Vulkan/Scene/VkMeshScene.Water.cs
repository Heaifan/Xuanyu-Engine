using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed partial class VkMeshScene
{
    private RenderWaterAtlas? _waterSource;
    private VkDemTexture? _waterTexture;

    public void SyncWater(IReadOnlyList<RenderDraw> draws, Queue queue, uint family)
    {
        RenderWaterAtlas? source = null;
        foreach (RenderDraw draw in draws)
            if (draw.Visible && draw.WaterAtlas is { } atlas)
            {
                source = atlas;
                break;
            }
        if (ReferenceEquals(source, _waterSource)) return;
        DisposeWater();
        if (source is null) return;
        _waterTexture = VkDemTexture.FromWaterAtlas(_vk, _gpu, _device, queue, family, source);
        _waterSource = source;
        Console.WriteLine($"[G06/VulkanSWB] 上传完成 {source.LoadedCount} 片，" +
            $"{source.Side}×{source.Side} R16_SINT，原始 1 角秒水陆位图");
    }

    public unsafe bool BindWater(CommandBuffer cmd, RenderDraw draw)
    {
        bool active = _waterTexture is not null && ReferenceEquals(draw.WaterAtlas, _waterSource);
        VkDemTexture texture = active ? _waterTexture! : _fallback!;
        DescriptorSet set = _descriptors.Get(texture);
        _vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, Layout, 1, 1, &set, 0, null);
        return active;
    }

    private void DisposeWater()
    {
        if (_waterTexture is not null) DestroyDem(_waterTexture);
        _waterTexture = null;
        _waterSource = null;
    }
}