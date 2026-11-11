
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkXlibSurface : IDisposable
{
    private readonly Instance _instance;
    private readonly KhrSurface _surfaceApi;
    private nint _display;
    private SurfaceKHR _surface;

    public SurfaceKHR Surface => _surface;

    public VkXlibSurface(Vk vk, Instance instance, nint xid)
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("Xlib Surface 仅支持 Linux。");
        if (!vk.TryGetInstanceExtension<KhrXlibSurface>(instance, out var xlib) ||
            !vk.TryGetInstanceExtension<KhrSurface>(instance, out var surfaceApi))
            throw new NotSupportedException("Vulkan Xlib Surface 扩展不可用。");

        _display = XOpenDisplay(0);
        if (_display == 0)
            throw new InvalidOperationException("无法连接 X11 Display。");
        _instance = instance;
        _surfaceApi = surfaceApi;
        try
        {
            var info = new XlibSurfaceCreateInfoKHR
            {
                SType = StructureType.XlibSurfaceCreateInfoKhr,
                Dpy = (nint*)_display,
                Window = xid
            };
            Result result = xlib.CreateXlibSurface(instance, &info, null, out _surface);
            if (result != Result.Success)
                throw new InvalidOperationException($"创建 Xlib SurfaceKHR 失败：{result}");
        }
        catch
        {
            XCloseDisplay(_display);
            _display = 0;
            throw;
        }
    }

    public void Dispose()
    {
        if (_surface.Handle != 0)
        {
            _surfaceApi.DestroySurface(_instance, _surface, null);
            _surface = default;
        }
        if (_display != 0)
        {
            XCloseDisplay(_display);
            _display = 0;
        }
    }

    [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    private static extern nint XOpenDisplay(nint name);

    [DllImport("libX11.so.6", EntryPoint = "XCloseDisplay")]
    private static extern int XCloseDisplay(nint display);
}
