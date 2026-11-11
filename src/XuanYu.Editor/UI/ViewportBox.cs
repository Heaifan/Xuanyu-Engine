using Avalonia;
using Avalonia.Controls;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportBox : Border
{
    private readonly Win32ViewportHost? _windows;
    private readonly X11ViewportHost? _linux;

    public nint ViewportHwnd => _windows?.Hwnd ?? 0;
    public (uint Width, uint Height) PixelSize =>
        _windows?.GetPixelSize() ?? _linux?.GetPixelSize() ?? (0u, 0u);

    public event Action<nint>? NativeWindowCreated
    {
        add
        {
            if (_windows is not null) _windows.HwndCreated += value;
            if (_linux is not null) _linux.XidCreated += value;
        }
        remove
        {
            if (_windows is not null) _windows.HwndCreated -= value;
            if (_linux is not null) _linux.XidCreated -= value;
        }
    }

    public event Action? NativeWindowDestroying
    {
        add
        {
            if (_windows is not null) _windows.HwndDestroying += value;
            if (_linux is not null) _linux.XidDestroying += value;
        }
        remove
        {
            if (_windows is not null) _windows.HwndDestroying -= value;
            if (_linux is not null) _linux.XidDestroying -= value;
        }
    }

    public event Action<int, int>? Clicked
    {
        add
        {
            if (_windows is not null) _windows.Clicked += value;
            if (_linux is not null) _linux.Clicked += value;
        }
        remove
        {
            if (_windows is not null) _windows.Clicked -= value;
            if (_linux is not null) _linux.Clicked -= value;
        }
    }

    internal event Action<ViewportCameraGesture>? CameraGesture
    {
        add
        {
            if (_windows is not null) _windows.CameraGesture += value;
            if (_linux is not null) _linux.CameraGesture += value;
        }
        remove
        {
            if (_windows is not null) _windows.CameraGesture -= value;
            if (_linux is not null) _linux.CameraGesture -= value;
        }
    }

    public ViewportBox(string statusNote)
    {
        Background = EditorTheme.ViewportBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        Margin = new Thickness(6, 0);
        if (OperatingSystem.IsWindows())
        {
            _windows = new Win32ViewportHost();
            Child = _windows;
        }
        else if (OperatingSystem.IsLinux())
        {
            _linux = new X11ViewportHost();
            Child = _linux;
        }
        else
            throw new PlatformNotSupportedException("仅支持 Windows 与 Linux 桌面平台。");
    }

    public void UpdateOverlay(string statusNote, string activePageTitle) =>
        UpdateOverlay(activePageTitle);

    public void UpdateOverlay(string activePageTitle) =>
        _windows?.SetPageTitle(activePageTitle);
}