using System.Runtime.InteropServices;

namespace XuanYu.Editor.UI;

internal sealed partial class Win32ViewportMouseHook
{
    private bool _panMode;
    private bool _leftPanDragging;

    public void SetPanMode(bool enabled)
    {
        _panMode = enabled;
        if (!enabled && _leftPanDragging)
        {
            _leftPanDragging = false;
            ReleaseCapture();
        }
    }

    private bool TryHandlePan(uint message, nint hwnd, int x, int y, nint param)
    {
        if (message == 0x0020 && _panMode &&
            (param.ToInt64() & 0xffff) == 1)
        {
            SetCursor(LoadCursorW(0, (nint)32649));
            return true;
        }
        if (!_panMode && !_leftPanDragging) return false;
        if (message == 0x0201 && _panMode)
        {
            SetFocus(hwnd);
            _leftPanDragging = true;
            _lastX = x; _lastY = y;
            SetCapture(hwnd);
            return true;
        }
        if (message == 0x0200 && _leftPanDragging)
        {
            _camera?.Invoke(new ViewportCameraGesture(
                x - _lastX, y - _lastY, 0f, true));
            _lastX = x; _lastY = y;
            return true;
        }
        if (message == 0x0202 && _leftPanDragging)
        {
            _leftPanDragging = false;
            ReleaseCapture();
            return true;
        }
        if (message is 0x0215 or 0x0008)
            _leftPanDragging = false;
        return false;
    }

    [DllImport("user32.dll", EntryPoint = "LoadCursorW")]
    private static extern nint LoadCursorW(nint instance, nint cursorName);
    [DllImport("user32.dll")]
    private static extern nint SetCursor(nint cursor);
}