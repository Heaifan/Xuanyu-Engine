using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

public sealed partial class VkTerrainPagePool
{
    internal (int Slot, int Mask)[] PrepareDraws(IReadOnlyList<TerrainPoolDraw> draws)
    {
        var result = new (int Slot, int Mask)[draws.Count];
        for (int i = 0; i < draws.Count; i++)
        {
            var draw = draws[i];
            if (!IsReady(draw.Key)) throw new InvalidOperationException($"G07 高程页尚未就绪：{draw.Key}");
            int slot = _slots[draw.Key];
            _bindings.SetParameters(slot, VkTerrainDrawParameters.Create(draw.South, draw.West,
                draw.Step, draw.Origin, draw.Mvp, draw.ClipRects, draw.Style, _waterSource, draw.RenderOrigin));
            result[i] = (slot, draw.Mask);
        }
        if (result.Length != 0) _bindings.FlushParameters();
        return result;
    }

    internal void Draw(Vk vk, CommandBuffer command, PipelineLayout layout, VkTerrainTopology topology,
        (int Slot, int Mask)[] draws)
    {
        foreach (var draw in draws)
        {
            _bindings.Bind(command, layout, draw.Slot);
            topology.Draw(vk, command, draw.Mask);
        }
    }
}