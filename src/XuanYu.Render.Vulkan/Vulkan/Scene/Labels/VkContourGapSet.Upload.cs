using System.Numerics;
using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkContourGapSet
{
    public void Sync(IReadOnlyList<RenderDraw> draws)
    {
        _ranges.Clear();
        int total = 0;
        foreach (var draw in draws)
            if (draw.Visible) total += draw.ContourLabelGaps.Length;
        EnsureCapacity(total);
        if (total == 0) return;
        void* pointer = null;
        ulong bytes = (ulong)total * (ulong)sizeof(Vector4);
        Check(_vk.MapMemory(_device, _memory, 0, bytes, 0, &pointer));
        try
        {
            Span<Vector4> buffer = new(pointer, total);
            int offset = 0;
            foreach (RenderDraw draw in draws)
            {
                if (!draw.Visible || draw.ContourLabelGaps.IsEmpty) continue;
                var source = draw.ContourLabelGaps;
                source.Span.CopyTo(buffer.Slice(offset, source.Length));
                _ranges[source] = ((uint)offset, (uint)source.Length);
                offset += source.Length;
            }
        }
        finally { _vk.UnmapMemory(_device, _memory); }
    }

    public (uint First, uint Count) Bind(CommandBuffer command,
        PipelineLayout layout, RenderDraw draw)
    {
        DescriptorSet set = Set;
        _vk.CmdBindDescriptorSets(command, PipelineBindPoint.Graphics,
            layout, 2, 1, &set, 0, null);
        return _ranges.TryGetValue(draw.ContourLabelGaps, out var range)
            ? range : (0u, 0u);
    }
}