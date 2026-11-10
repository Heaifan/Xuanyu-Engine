
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkWin32Surface : IDisposable
{
    private readonly Instance _instance;
    private readonly KhrSurface _surfaceApi;
    private SurfaceKHR _surface;

    public SurfaceKHR Surface => _surface;

    public VkWin32Surface(Vk vk, Instance instance, nint hwnd)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "Win32 Surface 仅支持 Windows。");

        if (!vk.TryGetInstanceExtension<KhrWin32Surface>(instance, out var win32) ||
            !vk.TryGetInstanceExtension<KhrSurface>(instance, out var surfaceApi))
            throw new NotSupportedException("Vulkan Surface 扩展不可用。");

        _instance = instance;
        _surfaceApi = surfaceApi;

        var createInfo = new Win32SurfaceCreateInfoKHR
        {
            SType = StructureType.Win32SurfaceCreateInfoKhr,
            Hinstance = GetModuleHandleW(null),
            Hwnd = hwnd
        };

        Result result = win32.CreateWin32Surface(
            instance, &createInfo, null, out _surface);

        if (result != Result.Success)
            throw new InvalidOperationException(
                $"创建 Win32 SurfaceKHR 失败：{result}");
    }

    public void Dispose()
    {
        if (_surface.Handle == 0)
            return;

        _surfaceApi.DestroySurface(_instance, _surface, null);
        _surface = default;
    }

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW",
        CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandleW(string? moduleName);
}
