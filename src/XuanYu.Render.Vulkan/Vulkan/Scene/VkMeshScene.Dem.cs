using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed partial class VkMeshScene
{
    private const int MaxDemTextures = 12;
    private readonly Dictionary<(int South, int West),
        (ReadOnlyMemory<short> Source, VkDemTexture Texture)> _demTextures = [];
    private VkDemTexture? _fallback;
    private bool _firstDemBindLogged;
    private static readonly bool DemContourEnabled =
        Environment.GetEnvironmentVariable("XYE_G06_DEM_CONTOUR") == "1";
    private static readonly bool DemUploadEnabled = DemContourEnabled ||
        Environment.GetEnvironmentVariable("XYE_G06_DEM_UPLOAD") == "1";

    public void SyncDem(IReadOnlyList<RenderDraw> draws, Queue queue, uint family)
    {
        _fallback ??= VkDemTexture.Fallback(_vk, _gpu, _device, queue, family);
        if (!DemUploadEnabled) return;
        var active = new HashSet<(int South, int West)>();
        foreach (RenderDraw draw in draws)
            if (draw.Visible && draw.ResidentDem is { } dem)
                active.Add((dem.SouthLatitude, dem.WestLongitude));
        foreach (var key in _demTextures.Keys.ToArray())
            if (!active.Contains(key))
            {
                DestroyDem(_demTextures[key].Texture);
                _demTextures.Remove(key);
            }
        foreach (RenderDraw draw in draws)
        {
            if (!draw.Visible || draw.ResidentDem is not { } dem) continue;
            var key = (dem.SouthLatitude, dem.WestLongitude);
            if (_demTextures.TryGetValue(key, out var old))
            {
                if (old.Source.Equals(dem.SamplesNorthToSouth)) continue;
                DestroyDem(old.Texture);
                _demTextures.Remove(key);
            }
            if (_demTextures.Count >= MaxDemTextures) break;
            var texture = new VkDemTexture(_vk, _gpu, _device,
                queue, family, dem);
            _demTextures.Add(key, (dem.SamplesNorthToSouth, texture));
            Console.WriteLine($"[G06/VulkanDEM] 上传完成 {dem.SouthLatitude},{dem.WestLongitude} " +
                $"3601×3601 R16_SINT，缓存 {_demTextures.Count}/{MaxDemTextures}");
            break;
        }
    }

    private void DestroyDem(VkDemTexture texture)
    {
        _descriptors.Release(texture);
        texture.Dispose();
    }

    private void DisposeDem()
    {
        foreach (var item in _demTextures.Values) DestroyDem(item.Texture);
        _demTextures.Clear();
        if (_fallback is not null) DestroyDem(_fallback);
        _fallback = null;
    }
}