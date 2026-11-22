using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed partial class VkMeshScene
{
    internal void SyncContourGaps(IReadOnlyList<RenderDraw> draws) =>
        _contourGaps.Sync(draws);

    internal (uint First, uint Count) BindContourGaps(CommandBuffer cmd,
        RenderDraw draw) => _contourGaps.Bind(cmd, Layout, draw);
}