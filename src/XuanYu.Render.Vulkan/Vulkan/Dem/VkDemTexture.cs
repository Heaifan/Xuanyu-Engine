using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkDemTexture : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private Image _image;
    private DeviceMemory _memory;
    private ImageView _view;
    private Sampler _sampler;

    public ImageView View => _view;
    public Sampler Sampler => _sampler;

    public VkDemTexture(Vk vk, PhysicalDevice gpu, Device device,
        Queue queue, uint family, RenderDemTile dem)
    {
        if (dem.SamplesNorthToSouth.Length != RenderDemTile.Side * RenderDemTile.Side)
            throw new ArgumentException("原始 DEM 样本数量必须为 3601×3601。", nameof(dem));
        _vk = vk;
        _device = device;
        try
        {
            CreateImage(gpu);
            CreateViewAndSampler();
            Upload(gpu, queue, family, dem.SamplesNorthToSouth);
        }
        catch
        {
            Dispose();
            throw;
        }
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