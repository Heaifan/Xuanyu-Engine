
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

public sealed unsafe class VkContext : IDisposable
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

    public void AttachWin32Surface(nint hwnd)
    {
        if (!IsInitialized)
            throw new ObjectDisposedException(nameof(VkContext));
        DetachWin32Surface();
        _presentation = new VkPresentationSession(_vk, _instance,
            PhysicalDevice, _device, GraphicsQueue,
            Gpu.GraphicsFamilyIndex, hwnd);
    }

    public string ConfigureSwapchain(uint width, uint height, bool force = false)
    {
        if (_presentation is null)
            throw new InvalidOperationException("SurfaceKHR 尚未创建。");
        return _presentation.Configure(width, height, force);
    }

    public Result PresentFrame() =>
        _presentation?.PresentFrame() ?? Result.NotReady;

    public void DetachWin32Surface()
    {
        _presentation?.Dispose();
        _presentation = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        DetachWin32Surface();
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
