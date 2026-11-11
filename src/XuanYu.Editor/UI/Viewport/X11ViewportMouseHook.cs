using Avalonia.Threading;

namespace XuanYu.Editor.UI;

internal sealed partial class X11ViewportMouseHook
{
    private readonly DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromMilliseconds(16)
    };
    private readonly byte[] _eventData = new byte[24 * IntPtr.Size];
    private nint _display;
    private nint _window;
    private Action<int, int>? _clicked;
    private Action<ViewportCameraGesture>? _camera;
    private bool _dragging;
    private int _lastX;
    private int _lastY;

    public X11ViewportMouseHook() => _timer.Tick += (_, _) => Poll();

    public void Attach(nint xid, Action<int, int> clicked,
        Action<ViewportCameraGesture> camera,
        Action<ViewportCameraShortcut> shortcut)
    {
        _display = XOpenDisplay(0);
        if (_display == 0)
            throw new InvalidOperationException("无法连接 X11 鼠标事件 Display。");
        _window = xid;
        _clicked = clicked;
        _camera = camera;
        _shortcut = shortcut;
        XSelectInput(_display, _window, 1 | 4 | 8 | 64);
        XFlush(_display);
        _timer.Start();
    }

    public void Detach()
    {
        _timer.Stop();
        if (_display != 0)
            XCloseDisplay(_display);
        _display = 0;
        _window = 0;
        _clicked = null;
        _camera = null;
        _shortcut = null;
        _dragging = false;
    }

    private void Poll()
    {
        // XEvent 中 Button/Motion 的 x/y 从第八个原生 long 起。
        int offset = 8 * IntPtr.Size;
        while (XPending(_display) > 0)
        {
            XNextEvent(_display, _eventData);
            int type = BitConverter.ToInt32(_eventData, 0);
            int x = BitConverter.ToInt32(_eventData, offset);
            int y = BitConverter.ToInt32(_eventData, offset + 4);
            int state = BitConverter.ToInt32(_eventData, offset + 16);
            int button = BitConverter.ToInt32(_eventData, offset + 20);
            if (type == 2)
                DispatchShortcut();
            else if (type == 4)
            {
                if (button is 1 or 2)
                    XSetInputFocus(_display, _window, 2, 0);
                if (button == 1) _clicked?.Invoke(x, y);
                if (button == 2)
                {
                    _dragging = true;
                    _lastX = x; _lastY = y;
                }
                if (button is 4 or 5)
                    _camera?.Invoke(new(0, 0, button == 4 ? 1f : -1f, false));
            }
            else if (type == 5 && button == 2)
                _dragging = false;
            else if (type == 6 && _dragging)
            {
                _camera?.Invoke(new(x - _lastX, y - _lastY,
                    0f, (state & 1) != 0));
                _lastX = x; _lastY = y;
            }
        }
    }
}