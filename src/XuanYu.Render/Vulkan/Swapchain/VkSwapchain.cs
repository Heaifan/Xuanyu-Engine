
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkSwapchain : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;
    private readonly KhrSurface _surfaceApi;
    private readonly KhrSwapchain _api;
    private readonly PhysicalDevice _gpu;
    private readonly SurfaceKHR _surface;
    private SwapchainKHR _handle;

    public SwapchainKHR Handle => _handle;
    public KhrSwapchain Api => _api;
    public Image[] Images { get; private set; } = [];
    public Extent2D Extent { get; private set; }
    public Format Format { get; private set; }

    public VkSwapchain(Vk vk, Instance instance, PhysicalDevice gpu,
        Device device, uint family, SurfaceKHR surface)
    {
        _vk = vk;
        _gpu = gpu;
        _device = device;
        _surface = surface;
        if (!vk.TryGetInstanceExtension<KhrSurface>(instance, out var surfaceApi) ||
            !vk.TryGetDeviceExtension<KhrSwapchain>(instance, device, out var swapchainApi))
            throw new NotSupportedException("Vulkan Swapchain 扩展不可用。");
        _surfaceApi = surfaceApi;
        _api = swapchainApi;
        Result result = _surfaceApi.GetPhysicalDeviceSurfaceSupport(
            gpu, family, surface, out var supported);
        if (result != Result.Success || !supported)
            throw new NotSupportedException(
                "选定的 Graphics Queue 不支持当前 Surface Present。");
    }

    public bool Configure(uint width, uint height, bool force = false)
    {
        var info = VkSwapchainOptions.Choose(
            _surfaceApi, _gpu, _surface, width, height);
        if (!force && _handle.Handle != 0 &&
            Extent.Width == info.ImageExtent.Width &&
            Extent.Height == info.ImageExtent.Height && Format == info.ImageFormat)
            return false;
        if (_handle.Handle != 0 && _vk.DeviceWaitIdle(_device) != Result.Success)
            throw new InvalidOperationException("等待 Vulkan Device 空闲失败。");
        info.OldSwapchain = _handle;
        Result result = _api.CreateSwapchain(_device, &info, null, out var next);
        if (result != Result.Success)
            throw new InvalidOperationException($"创建 SwapchainKHR 失败：{result}");
        uint count = 0;
        result = _api.GetSwapchainImages(_device, next, &count, null);
        if (result != Result.Success || count == 0)
        {
            _api.DestroySwapchain(_device, next, null);
            throw new InvalidOperationException($"获取交换链图像数量失败：{result}");
        }
        var images = new Image[checked((int)count)];
        fixed (Image* pointer = images)
            result = _api.GetSwapchainImages(_device, next, &count, pointer);
        if (result != Result.Success)
        {
            _api.DestroySwapchain(_device, next, null);
            throw new InvalidOperationException($"获取交换链图像失败：{result}");
        }
        if (_handle.Handle != 0)
            _api.DestroySwapchain(_device, _handle, null);
        _handle = next;
        Images = images;
        Extent = info.ImageExtent;
        Format = info.ImageFormat;
        return true;
    }

    public void Dispose()
    {
        if (_handle.Handle == 0)
            return;
        _vk.DeviceWaitIdle(_device);
        _api.DestroySwapchain(_device, _handle, null);
        _handle = default;
        Images = [];
    }
}
