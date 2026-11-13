using System.ComponentModel;
using System.Runtime.InteropServices;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.UI;

internal sealed partial class Win32ViewportMouseHook
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint WindowProcedure(
        nint hwnd, uint message, nuint wParam, nint lParam);
    private WindowProcedure? _callback;
    private Action<int, int>? _clicked;
    private Action<ViewportCameraGesture>? _camera;
    private Action<ViewportCameraShortcut>? _shortcut;
    private nint _hwnd;
    private nint _previous;
    private bool _dragging;
    private int _lastX, _lastY;
    public void Attach(nint hwnd, Action<int, int> clicked,
        Action<ViewportCameraGesture> camera,
        Action<ViewportCameraShortcut> shortcut,
        Action<GameCameraKey, bool> gameInput)
    {
        _hwnd = hwnd;
        _clicked = clicked;
        _camera = camera;
        _shortcut = shortcut;
        _gameInput = gameInput;
        _callback = OnMessage;
        _previous = SetWindowLongPtrW(
            hwnd, -4, Marshal.GetFunctionPointerForDelegate(_callback));
        if (_previous == 0)
            throw new Win32Exception(
                Marshal.GetLastWin32Error(), "注册视口鼠标事件失败。");
    }
    public void Detach()
    {
        if (_previous == 0) return;
        SetPanMode(false);
        nint result = SetWindowLongPtrW(_hwnd, -4, _previous);
        if (result == 0)
            throw new Win32Exception(
                Marshal.GetLastWin32Error(), "恢复视口窗口过程失败。");
        _previous = 0;
        _callback = null; _clicked = null; _camera = null;
        _shortcut = null; _gameInput = null;
        _dragging = false; _hwnd = 0;
    }
    private nint OnMessage(nint hwnd, uint message, nuint wParam, nint lParam)
    {
        int packed = unchecked((int)lParam.ToInt64());
        int x = (short)packed, y = (short)(packed >> 16);
        if (TryHandlePan(message, hwnd, x, y, lParam))
            return message == 0x0020 ? (nint)1 : 0;
        if (message == 0x0008)
            _gameInput?.Invoke(GameCameraKey.None, false);
        if (message == 0x0100 && DispatchShortcut(wParam, lParam))
            return 0;
        if ((message == 0x0100 || message == 0x0101) &&
            DispatchGameInput(wParam, message == 0x0100))
            return 0;
        if (message == 0x0201)
        {
            SetFocus(hwnd);
            _clicked?.Invoke(x, y);
            return 0;
        }
        if (message == 0x0207)
        {
            SetFocus(hwnd);
            _dragging = true;
            _lastX = x; _lastY = y;
            SetCapture(hwnd);
            return 0;
        }
        if (message == 0x0208)
        {
            _dragging = false;
            ReleaseCapture();
            return 0;
        }
        if (message == 0x0215) _dragging = false;
        if (message == 0x0200 && _dragging)
        {
            _camera?.Invoke(new ViewportCameraGesture(
                x - _lastX, y - _lastY, 0f,
                ((ulong)wParam & 4ul) != 0));
            _lastX = x; _lastY = y;
            return 0;
        }
        if (message == 0x020A)
        {
            float steps = (short)((ulong)wParam >> 16) / 120f;
            _camera?.Invoke(new ViewportCameraGesture(0f, 0f, steps, false));
            return 0;
        }
        return CallWindowProcW(_previous, hwnd, message, wParam, lParam);
    }
}