using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkContext : IDisposable
{
    private readonly Vk _vk;
    private Instance _instance;
    private Device _device;
    private VkPresentationSession? _presentation;
    private bool _disposed;

    public PhysicalDevice PhysicalDevice { get; private set; }
    public Queue GraphicsQueue { get; private set; }
    public GpuInfo Gpu { get; private set; } = null!;
    public bool HasSurface => _presentation is not null;
    public bool IsInitialized =>
        !_disposed && _instance.Handle != 0 && _device.Handle != 0;

    public VkContext()
    {
        _vk = Vk.GetApi();
        try
        {
            _instance = VkInstanceCreator.Create(_vk);
            (PhysicalDevice, Gpu) = VkPhysicalDeviceSelector.Pick(_vk, _instance);
            (_device, GraphicsQueue) = VkLogicalDeviceCreator.Create(
                _vk, PhysicalDevice, Gpu.GraphicsFamilyIndex);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void AttachSurface(nint nativeWindow)
    {
        if (!IsInitialized)
            throw new ObjectDisposedException(nameof(VkContext));
        DetachSurface();
        _presentation = new VkPresentationSession(
            _vk, _instance, PhysicalDevice, _device,
            GraphicsQueue, Gpu.GraphicsFamilyIndex, nativeWindow);
    }

    public void AttachWin32Surface(nint hwnd) => AttachSurface(hwnd);

    public string ConfigureSwapchain(uint width, uint height, bool force = false)
    {
        if (_presentation is null)
            throw new InvalidOperationException("SurfaceKHR 尚未创建。");
        return _presentation.Configure(width, height, force);
    }

    public Result PresentFrame(IReadOnlyList<RenderDraw> draws) =>
        _presentation?.PresentFrame(draws) ?? Result.NotReady;

    public void DetachSurface()
    {
        _presentation?.Dispose();
        _presentation = null;
    }

    public void DetachWin32Surface() => DetachSurface();

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        DetachSurface();
        if (_device.Handle != 0)
        {
            _vk.DeviceWaitIdle(_device);
            _vk.DestroyDevice(_device, null);
            _device = default;
        }
        if (_instance.Handle != 0)
        {
            _vk.DestroyInstance(_instance, null);
            _instance = default;
        }
        _vk.Dispose();
    }
}