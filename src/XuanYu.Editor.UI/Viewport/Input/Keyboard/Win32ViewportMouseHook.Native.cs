using System.Runtime.InteropServices;

namespace XuanYu.Editor.UI;

internal sealed partial class Win32ViewportMouseHook
{
    private bool DispatchShortcut(nuint code, nint parameter)
    {
        if ((uint)code != 0x24 && (uint)code != 0x46)
            return false;
        if ((parameter.ToInt64() & (1L << 30)) == 0)
            _shortcut?.Invoke((uint)code == 0x24
                ? ViewportCameraShortcut.Home
                : ViewportCameraShortcut.FocusSelection);
        return true;
    }

    private nint HandleWheel(nint hwnd, nuint wParam, int x, int y)
    {
        var point = new MousePoint { X = x, Y = y };
        ScreenToClient(hwnd, ref point);
        float steps = (short)((ulong)wParam >> 16) / 120f;
        _camera?.Invoke(new ViewportCameraGesture(0f, 0f, steps,
            false, point.X, point.Y));
        return 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MousePoint { public int X; public int Y; }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient(nint hwnd, ref MousePoint point);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtrW(nint hwnd, int index, nint value);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern nint CallWindowProcW(nint previous, nint hwnd,
        uint message, nuint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern nint SetCapture(nint hwnd);

    [DllImport("user32.dll")]
    private static extern nint SetFocus(nint hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReleaseCapture();
}