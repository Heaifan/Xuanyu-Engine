
using Avalonia.Threading;
using Silk.NET.Vulkan;
using XuanYu.Editor.Pages;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

internal sealed class ViewportSwapchainSession
{
    private readonly ViewportBox _viewport;
    private readonly VkContext _vk;
    private readonly Workbench _workbench;
    private readonly DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromMilliseconds(50)
    };
    private bool _firstFrame;

    public ViewportSwapchainSession(
        ViewportBox viewport, VkContext vk, Workbench workbench)
    {
        _viewport = viewport;
        _vk = vk;
        _workbench = workbench;
        _viewport.NativeWindowCreated += OnNativeWindowCreated;
        _viewport.NativeWindowDestroying += OnNativeWindowDestroying;
        _timer.Tick += (_, _) => Update();
    }

    private void OnNativeWindowCreated(nint window)
    {
        try
        {
            _vk.AttachSurface(window);
            string platform = OperatingSystem.IsWindows() ? "Win32" : "Xlib";
            _workbench.Log.Write($"Vulkan {platform} SurfaceKHR 创建成功");
            _firstFrame = false;
            _timer.Start();
            Dispatcher.UIThread.Post(Update, DispatcherPriority.Loaded);
        }
        catch (Exception error)
        {
            _workbench.Log.Write($"SurfaceKHR 创建失败：{error.Message}");
        }
    }

    private void Update()
    {
        if (!_vk.HasSurface)
            return;
        var (width, height) = _viewport.PixelSize;
        if (width == 0 || height == 0)
            return;
        try
        {
            string summary = _vk.ConfigureSwapchain(width, height);
            if (summary.Length > 0)
                _workbench.Log.Write(summary);
            Result result = _vk.PresentFrame(_workbench.Scene.Draws);
            if (result == Result.ErrorOutOfDateKhr)
            {
                summary = _vk.ConfigureSwapchain(width, height, force: true);
                _workbench.Log.Write(summary);
                _firstFrame = false;
                return;
            }
            if (result == Result.Success || result == Result.SuboptimalKhr)
            {
                if (!_firstFrame)
                    _workbench.Log.Write("Vulkan 首帧多对象 GPU Draw 与 Present 提交成功");
                _firstFrame = true;
            }
        }
        catch (Exception error)
        {
            _timer.Stop();
            _workbench.Log.Write($"Vulkan 帧呈现失败：{error.Message}");
        }
    }

    private void OnNativeWindowDestroying()
    {
        _timer.Stop();
        _vk.DetachSurface();
    }
}
