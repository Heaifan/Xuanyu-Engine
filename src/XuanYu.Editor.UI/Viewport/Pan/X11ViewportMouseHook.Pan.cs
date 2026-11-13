using System.Runtime.InteropServices;

namespace XuanYu.Editor.UI;

internal sealed partial class X11ViewportMouseHook
{
    private bool _panMode;
    private bool _leftPanDragging;
    private nuint _handCursor;

    public void SetPanMode(bool enabled)
    {
        _panMode = enabled;
        _leftPanDragging = false;
        if (_display == 0) return;
        if (enabled)
        {
            _handCursor = XCreateFontCursor(_display, 60);
            XDefineCursor(_display, _window, _handCursor);
        }
        else
        {
            XUndefineCursor(_display, _window);
            if (_handCursor != 0) XFreeCursor(_display, _handCursor);
            _handCursor = 0;
        }
        XFlush(_display);
    }

    private bool TryHandlePan(int type, int button, int x, int y)
    {
        if (!_panMode) return false;
        if (type == 4 && button == 1)
        {
            XSetInputFocus(_display, _window, 2, 0);
            _leftPanDragging = true;
            _lastX = x; _lastY = y;
            return true;
        }
        if (type == 6 && _leftPanDragging)
        {
            _camera?.Invoke(new ViewportCameraGesture(
                x - _lastX, y - _lastY, 0f, true));
            _lastX = x; _lastY = y;
            return true;
        }
        if (type == 5 && button == 1 && _leftPanDragging)
        {
            _leftPanDragging = false;
            return true;
        }
        return false;
    }

    [DllImport("libX11.so.6")]
    private static extern nuint XCreateFontCursor(nint display, uint shape);
    [DllImport("libX11.so.6")]
    private static extern int XDefineCursor(nint display, nint window, nuint cursor);
    [DllImport("libX11.so.6")]
    private static extern int XUndefineCursor(nint display, nint window);
    [DllImport("libX11.so.6")]
    private static extern int XFreeCursor(nint display, nuint cursor);
}