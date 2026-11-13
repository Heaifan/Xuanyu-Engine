using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.UI;

internal sealed class Win32ViewportHost : NativeControlHost
{
    private const uint WindowStyle =
        0x40000000 | 0x10000000 | 0x04000000 |
        0x02000000 | 0x00000004 | 0x00000100;
    private readonly Win32ViewportMouseHook _mouse = new();
    private string _pageTitle = "World";

    public nint Hwnd { get; private set; }
    public event Action<nint>? HwndCreated;
    public event Action? HwndDestroying;
    public event Action<int, int>? Clicked;
    public event Action<ViewportCameraGesture>? CameraGesture;
    public event Action<ViewportCameraShortcut>? CameraShortcut;
    public event Action<GameCameraKey, bool>? GameInput;
    public void SetPanMode(bool enabled) => _mouse.SetPanMode(enabled);

    public (uint Width, uint Height) GetPixelSize()
    {
        if (Hwnd == 0 || !GetClientRect(Hwnd, out var rect))
            return (0, 0);
        return ((uint)Math.Max(0, rect.Right - rect.Left),
                (uint)Math.Max(0, rect.Bottom - rect.Top));
    }

    public void SetPageTitle(string title)
    {
        _pageTitle = title;
        if (Hwnd != 0)
            SetWindowTextW(Hwnd, $"XuanYu Viewport - {_pageTitle}");
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows 视口需要 HWND。");
        nint hwnd = CreateWindowExW(
            0, "STATIC", $"XuanYu Viewport - {_pageTitle}",
            WindowStyle, 0, 0, 1, 1, parent.Handle, 0, 0, 0);
        if (hwnd == 0)
            throw new Win32Exception(
                Marshal.GetLastWin32Error(), "创建视口 HWND 失败。");
        Hwnd = hwnd;
        try
        {
            _mouse.Attach(hwnd,
                (x, y) => Clicked?.Invoke(x, y),
                gesture => CameraGesture?.Invoke(gesture),
                shortcut => CameraShortcut?.Invoke(shortcut),
                (key, down) => GameInput?.Invoke(key, down));
        }
        catch
        {
            DestroyWindow(hwnd);
            Hwnd = 0;
            throw;
        }
        Console.WriteLine($"[XYE] 视口 HWND 已创建：0x{hwnd.ToInt64():X}");
        HwndCreated?.Invoke(hwnd);
        return new PlatformHandle(hwnd, "HWND");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        HwndDestroying?.Invoke();
        _mouse.Detach();
        if (!DestroyWindow(control.Handle))
            Console.Error.WriteLine(
                $"[XYE] 视口 HWND 销毁失败：{Marshal.GetLastWin32Error()}");
        Hwnd = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(nint hwnd, out Rect rect);
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW",
        CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateWindowExW(
        uint exStyle, string className, string windowName, uint style,
        int x, int y, int width, int height,
        nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint hwnd);
    [DllImport("user32.dll", EntryPoint = "SetWindowTextW",
        CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowTextW(nint hwnd, string text);
}