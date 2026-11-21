using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed partial class VkMeshScene
{
    private const int MaxDemTextures = 12;
    private readonly Dictionary<(int South, int West),
        (ReadOnlyMemory<short> Source, VkDemTexture Texture)> _demTextures = [];
    private static readonly bool DemUploadEnabled =
        Environment.GetEnvironmentVariable("XYE_G06_DEM_UPLOAD") == "1";

    public void SyncDem(IReadOnlyList<RenderDraw> draws, Queue queue, uint family)
    {
        if (!DemUploadEnabled) return;
        var active = new HashSet<(int South, int West)>();
        foreach (RenderDraw draw in draws)
            if (draw.Visible && draw.ResidentDem is { } dem)
                active.Add((dem.SouthLatitude, dem.WestLongitude));
        foreach (var key in _demTextures.Keys.ToArray())
            if (!active.Contains(key))
            {
                _demTextures[key].Texture.Dispose();
                _demTextures.Remove(key);
            }
        foreach (RenderDraw draw in draws)
        {
            if (!draw.Visible || draw.ResidentDem is not { } dem) continue;
            var key = (dem.SouthLatitude, dem.WestLongitude);
            if (_demTextures.TryGetValue(key, out var old))
            {
                if (old.Source.Equals(dem.SamplesNorthToSouth)) continue;
                old.Texture.Dispose();
                _demTextures.Remove(key);
            }
            if (_demTextures.Count >= MaxDemTextures) break;
            var texture = new VkDemTexture(_vk, _gpu, _device,
                queue, family, dem);
            _demTextures.Add(key, (dem.SamplesNorthToSouth, texture));
            Console.WriteLine($"[G06/VulkanDEM] 上传完成 {dem.SouthLatitude},{dem.WestLongitude} " +
                $"3601×3601 R16_SINT，缓存 {_demTextures.Count}/{MaxDemTextures}");
            break; // 单帧至多上传一张原始纹理。
        }
    }

    private void DisposeDem()
    {
        foreach (var item in _demTextures.Values)
            item.Texture.Dispose();
        _demTextures.Clear();
    }
}