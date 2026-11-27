using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkDemTexture : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly uint _side;
    private Image _image;
    private DeviceMemory _memory;
    private ImageView _view;
    private Sampler _sampler;
    public ImageView View => _view;
    public Sampler Sampler => _sampler;

    public VkDemTexture(Vk vk, PhysicalDevice gpu, Device device,
        Queue queue, uint family, RenderDemTile dem)
        : this(vk, gpu, device, queue, family,
            dem.SamplesNorthToSouth, RenderDemTile.Side) { }

    public static VkDemTexture Fallback(Vk vk, PhysicalDevice gpu, Device device,
        Queue queue, uint family) =>
        new(vk, gpu, device, queue, family, new short[1], 1);

    public static VkDemTexture FromWaterAtlas(Vk vk, PhysicalDevice gpu, Device device,
        Queue queue, uint family, RenderWaterAtlas atlas) =>
        new(vk, gpu, device, queue, family, atlas.PackedSamples, (uint)atlas.Side);

    public static VkDemTexture FromTerrainPage(Vk vk, PhysicalDevice gpu, Device device,
        Queue queue, uint family, ReadOnlyMemory<short> samples) =>
        new(vk, gpu, device, queue, family, samples, 33, true);

    private VkDemTexture(Vk vk, PhysicalDevice gpu, Device device,
        Queue queue, uint family, ReadOnlyMemory<short> samples, uint side, bool vertexSampling = false)
    {
        if (samples.Length != (int)(side * side))
            throw new ArgumentException("DEM 纹理样本数与边长不匹配。", nameof(samples));
        _vk = vk;
        _device = device;
        _side = side;
        try
        {
            CreateImage(gpu);
            CreateViewAndSampler();
            Upload(gpu, queue, family, samples, vertexSampling);
        }
        catch { Dispose(); throw; }
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }

    public void Dispose()
    {
        if (_sampler.Handle != 0) _vk.DestroySampler(_device, _sampler, null);
        if (_view.Handle != 0) _vk.DestroyImageView(_device, _view, null);
        if (_image.Handle != 0) _vk.DestroyImage(_device, _image, null);
        if (_memory.Handle != 0) _vk.FreeMemory(_device, _memory, null);
        _sampler = default;
        _view = default;
        _image = default;
        _memory = default;
    }
}