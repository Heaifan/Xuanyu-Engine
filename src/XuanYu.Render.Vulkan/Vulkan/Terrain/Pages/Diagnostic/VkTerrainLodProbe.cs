using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

public sealed class VkTerrainLodProbe : IDisposable
{
    private readonly Vk _vk;
    private readonly Extent2D _extent = new(128, 128);
    private readonly VkTerrainOffscreenImage? _image;
    private readonly VkTrianglePass? _pass;
    private readonly VkTriangleTargets? _targets;
    private readonly VkTerrainDrawPipeline? _pipeline;
    private readonly VkTerrainTopology? _indices;
    private readonly VkTerrainPagePool? _pool;
    private readonly VkTerrainBuffer? _readback;
    private readonly VkTerrainDrawProbe? _probe;
    public VkTerrainPagePool Pool => _pool!;

    internal VkTerrainLodProbe(Vk vk, PhysicalDevice gpu, Device device, Queue queue, uint family,
        int capacity, int uploadLimit, ulong budget, IReadOnlyList<int[]> topology)
    {
        _vk = vk;
        try
        {
            _image = new(vk, gpu, device, _extent);
            _pass = new(vk, device, VkTerrainOffscreenImage.ColorFormat, true);
            _targets = new(vk, gpu, device, _extent, VkTerrainOffscreenImage.ColorFormat, [_image.Handle], _pass.Handle);
            _pipeline = new(vk, device, _pass.Handle, _extent);
            _indices = new(vk, gpu, device, topology);
            _pool = new(vk, gpu, device, queue, family, _pipeline.SetLayout, capacity, uploadLimit, budget);
            _readback = new(vk, gpu, device, 128 * 128 * 4, BufferUsageFlags.TransferDstBit);
            _probe = new(vk, device, queue, family);
        }
        catch { Dispose(); throw; }
    }

    public byte[] Draw(IReadOnlyList<TerrainPoolDraw> draws)
    {
        var prepared = Pool.PrepareDraws(draws);
        return _probe!.DrawCommands(_pass!.Handle, _targets![0], _extent, _image!.Handle,
            _readback!, _pipeline!, cmd => Pool.Draw(_vk, cmd, _pipeline!.Layout, _indices!, prepared));
    }

    public void Dispose()
    {
        _probe?.Dispose();
        _pool?.Dispose();
        _readback?.Dispose();
        _indices?.Dispose();
        _pipeline?.Dispose();
        _targets?.Dispose();
        _pass?.Dispose();
        _image?.Dispose();
    }
}