using System.Runtime.InteropServices;

namespace XuanYu.Editor.UI;

internal sealed partial class X11ViewportMouseHook
{
    private Action<ViewportCameraShortcut>? _shortcut;

    private void DispatchShortcut()
    {
        nint symbol = XLookupKeysym(_eventData, 0);
        if (symbol == 0xff50)
            _shortcut?.Invoke(ViewportCameraShortcut.Home);
        else if (symbol == 0x66 || symbol == 0x46)
            _shortcut?.Invoke(ViewportCameraShortcut.FocusSelection);
    }

    [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    private static extern nint XOpenDisplay(nint name);
    [DllImport("libX11.so.6", EntryPoint = "XSelectInput")]
    private static extern int XSelectInput(nint display, nint window, nint mask);
    [DllImport("libX11.so.6", EntryPoint = "XSetInputFocus")]
    private static extern int XSetInputFocus(nint display, nint focus,
        int revertTo, nint time);
    [DllImport("libX11.so.6", EntryPoint = "XLookupKeysym")]
    private static extern nint XLookupKeysym([In] byte[] keyEvent, int index);
    [DllImport("libX11.so.6", EntryPoint = "XFlush")]
    private static extern int XFlush(nint display);
    [DllImport("libX11.so.6", EntryPoint = "XPending")]
    private static extern int XPending(nint display);
    [DllImport("libX11.so.6", EntryPoint = "XNextEvent")]
    private static extern int XNextEvent(nint display, [Out] byte[] data);
    [DllImport("libX11.so.6", EntryPoint = "XCloseDisplay")]
    private static extern int XCloseDisplay(nint display);
}