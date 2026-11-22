using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed partial class VkMeshScene
{
    public unsafe bool BindDem(CommandBuffer cmd, RenderDraw draw)
    {
        VkDemTexture texture = _fallback!;
        bool bound = false;
        if ((DemContourEnabled || DemHillshadeEnabled) && draw.ResidentDem is { } dem &&
            !draw.DemUV.IsEmpty && _demTextures.TryGetValue(
                (dem.SouthLatitude, dem.WestLongitude), out var item))
        {
            texture = item.Texture;
            bound = true;
        }
        DescriptorSet set = _descriptors.Get(texture);
        _vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics,
            Layout, 0, 1, &set, 0, null);
        if (bound && !_firstDemBindLogged)
        {
            _firstDemBindLogged = true;
            Console.WriteLine($"[G06/VulkanDEM] 原始 DEM 描述符已绑定，" +
                $"RawContour={DemContourEnabled} PixelHillshade={DemHillshadeEnabled}");
        }
        return bound;
    }
}