using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkSharedColorProbe
{
    private VkTerrainViewportDraw? _terrain;

    private VkMeshScene EnsureScene(uint width, uint height) =>
        _scene ??= new VkMeshScene(_vk, _gpu, _device,
            Format.R8G8B8A8Unorm, new Extent2D(width, height),
            [_image], true);

    public void ResetTerrainPool()
    {
        _terrain?.Dispose();
        _terrain = null;
    }

    public VkTerrainPagePool GetOrCreateTerrainPool(uint width, uint height,
        int capacity, int uploadLimit, ulong budget, IReadOnlyList<int[]> topology, RenderWaterAtlas? water = null)
    {
        _terrain ??= new VkTerrainViewportDraw(_vk, _gpu, _device, _queue, _family,
            EnsureScene(width, height), new Extent2D(width, height),
            capacity, uploadLimit, budget, topology, water);
        return _terrain.Pool;
    }
}