
using System.Runtime.InteropServices;
using Avalonia.Threading;

namespace XuanYu.Editor.UI;

internal sealed class X11ViewportMouseHook
{
    private readonly DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromMilliseconds(16)
    };
    private readonly byte[] _eventData = new byte[24 * IntPtr.Size];
    private nint _display;
    private nint _window;
    private Action<int, int>? _clicked;

    public X11ViewportMouseHook() => _timer.Tick += (_, _) => Poll();

    public void Attach(nint xid, Action<int, int> clicked)
    {
        _display = XOpenDisplay(0);
        if (_display == 0)
            throw new InvalidOperationException("无法连接 X11 鼠标事件 Display。");
        _window = xid;
        _clicked = clicked;
        XSelectInput(_display, _window, 4);
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
    }

    private void Poll()
    {
        // XEvent 是 24 个 C long；XButtonEvent 的 x/y 从第 8 个 long 起。
        int offset = 8 * IntPtr.Size;
        while (XCheckTypedWindowEvent(_display, _window, 4, _eventData) != 0)
        {
            if (BitConverter.ToUInt32(_eventData, offset + 20) != 1)
                continue;
            int x = BitConverter.ToInt32(_eventData, offset);
            int y = BitConverter.ToInt32(_eventData, offset + 4);
            _clicked?.Invoke(x, y);
        }
    }

    [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    private static extern nint XOpenDisplay(nint name);

    [DllImport("libX11.so.6", EntryPoint = "XSelectInput")]
    private static extern int XSelectInput(nint display, nint window, nint mask);

    [DllImport("libX11.so.6", EntryPoint = "XFlush")]
    private static extern int XFlush(nint display);

    [DllImport("libX11.so.6", EntryPoint = "XCheckTypedWindowEvent")]
    private static extern int XCheckTypedWindowEvent(
        nint display, nint window, int type, [Out] byte[] eventData);

    [DllImport("libX11.so.6", EntryPoint = "XCloseDisplay")]
    private static extern int XCloseDisplay(nint display);
}
