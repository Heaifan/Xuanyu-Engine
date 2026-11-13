using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed class VkPresentationSession : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly Queue _queue;
    private readonly uint _family;
    private readonly PhysicalDevice _gpu;
    private readonly IDisposable _surfaceOwner;
    private readonly VkSwapchain _swapchain;
    private VkFrameRenderer? _frames;

    public VkPresentationSession(Vk vk, Instance instance,
        PhysicalDevice gpu, Device device, Queue queue, uint family, nint window)
    {
        _vk = vk;
        _device = device;
        _queue = queue;
        _family = family;
        _gpu = gpu;
        SurfaceKHR surface;
        if (OperatingSystem.IsWindows())
        {
            var win32 = new VkWin32Surface(vk, instance, window);
            _surfaceOwner = win32;
            surface = win32.Surface;
        }
        else if (OperatingSystem.IsLinux())
        {
            var xlib = new VkXlibSurface(vk, instance, window);
            _surfaceOwner = xlib;
            surface = xlib.Surface;
        }
        else
            throw new PlatformNotSupportedException("不支持当前 Vulkan 视口平台。");
        try
        {
            _swapchain = new VkSwapchain(vk, instance, gpu, device, family, surface);
        }
        catch
        {
            _surfaceOwner.Dispose();
            throw;
        }
    }

    public string Configure(uint width, uint height, bool force)
    {
        if (!_swapchain.Configure(width, height, force, ReleaseFrames))
            return "";
        _frames = new VkFrameRenderer(
            _vk, _gpu, _device, _queue, _family, _swapchain);
        return $"SwapchainKHR 与 Mesh Pipeline 创建成功：" +
            $"{_swapchain.Extent.Width}×{_swapchain.Extent.Height} | " +
            $"{_swapchain.Format} | MSAA 4x";
    }

    private void ReleaseFrames()
    {
        _frames?.Dispose();
        _frames = null;
    }

    public Result PresentFrame(IReadOnlyList<RenderDraw> draws) =>
        _frames?.PresentFrame(draws) ?? Result.NotReady;

    public void Dispose()
    {
        _vk.DeviceWaitIdle(_device);
        ReleaseFrames();
        _swapchain.Dispose();
        _surfaceOwner.Dispose();
    }
}