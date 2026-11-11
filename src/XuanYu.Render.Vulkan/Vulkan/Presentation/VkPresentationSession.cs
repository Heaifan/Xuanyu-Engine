
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed class VkPresentationSession : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly Queue _queue;
    private readonly uint _family;
    private readonly PhysicalDevice _gpu;
    private readonly VkWin32Surface _surface;
    private readonly VkSwapchain _swapchain;
    private VkFrameRenderer? _frames;

    public VkPresentationSession(Vk vk, Instance instance,
        PhysicalDevice gpu, Device device, Queue queue, uint family, nint hwnd)
    {
        _vk = vk;
        _device = device;
        _queue = queue;
        _family = family;
        _gpu = gpu;

        _surface = new VkWin32Surface(vk, instance, hwnd);
        try
        {
            _swapchain = new VkSwapchain(
                vk, instance, gpu, device, family, _surface.Surface);
        }
        catch
        {
            _surface.Dispose();
            throw;
        }
    }

    public string Configure(uint width, uint height, bool force)
    {
        if (!_swapchain.Configure(width, height, force, ReleaseFrames))
            return "";

        _frames = new VkFrameRenderer(
            _vk, _gpu, _device, _queue, _family, _swapchain);

        return $"SwapchainKHR 与 Triangle Pipeline 创建成功：" +
            $"{_swapchain.Extent.Width}×{_swapchain.Extent.Height} | " +
            $"{_swapchain.Format}";
    }

    private void ReleaseFrames()
    {
        _frames?.Dispose();
        _frames = null;
    }

    public Result PresentFrame() =>
        _frames?.PresentFrame() ?? Result.NotReady;

    public void Dispose()
    {
        _vk.DeviceWaitIdle(_device);
        ReleaseFrames();
        _swapchain.Dispose();
        _surface.Dispose();
    }
}
