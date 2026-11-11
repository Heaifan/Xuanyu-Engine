
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace XuanYu.Editor.UI;

internal sealed class Win32ViewportMouseHook
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint WindowProcedure(
        nint hwnd, uint message, nuint wParam, nint lParam);

    private WindowProcedure? _callback;
    private Action<int, int>? _clicked;
    private nint _hwnd;
    private nint _previous;

    public void Attach(nint hwnd, Action<int, int> clicked)
    {
        _hwnd = hwnd;
        _clicked = clicked;
        _callback = OnMessage;
        _previous = SetWindowLongPtrW(
            hwnd, -4, Marshal.GetFunctionPointerForDelegate(_callback));
        if (_previous == 0)
            throw new Win32Exception(
                Marshal.GetLastWin32Error(), "注册视口鼠标事件失败。");
    }

    public void Detach()
    {
        if (_previous == 0)
            return;
        nint result = SetWindowLongPtrW(_hwnd, -4, _previous);
        if (result == 0)
            throw new Win32Exception(
                Marshal.GetLastWin32Error(), "恢复视口窗口过程失败。");
        _previous = 0;
        _callback = null;
        _clicked = null;
        _hwnd = 0;
    }

    private nint OnMessage(nint hwnd, uint message, nuint wParam, nint lParam)
    {
        if (message == 0x0201)
        {
            int packed = unchecked((int)lParam.ToInt64());
            _clicked?.Invoke((short)packed, (short)(packed >> 16));
            return 0;
        }
        return CallWindowProcW(_previous, hwnd, message, wParam, lParam);
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtrW(
        nint hwnd, int index, nint newValue);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern nint CallWindowProcW(
        nint previous, nint hwnd, uint message, nuint wParam, nint lParam);
}
