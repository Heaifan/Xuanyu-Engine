using Avalonia.Controls;
using Avalonia.Platform;

namespace XuanYu.Editor.UI;

internal sealed class X11ViewportHost : NativeControlHost
{
    private readonly X11ViewportMouseHook _mouse = new();

    public nint Xid { get; private set; }
    public event Action<nint>? XidCreated;
    public event Action? XidDestroying;
    public event Action<int, int>? Clicked;
    public event Action<ViewportCameraGesture>? CameraGesture;
    public event Action<ViewportCameraShortcut>? CameraShortcut;

    public (uint Width, uint Height) GetPixelSize()
    {
        if (Xid == 0)
            return (0, 0);
        double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        return (
            (uint)Math.Max(0, (int)(Bounds.Width * scale)),
            (uint)Math.Max(0, (int)(Bounds.Height * scale)));
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (parent.HandleDescriptor != "XID")
            throw new PlatformNotSupportedException("Linux 视口需要 X11/XWayland。");
        IPlatformHandle control = base.CreateNativeControlCore(parent);
        Xid = control.Handle;
        try
        {
            _mouse.Attach(Xid, (x, y) => Clicked?.Invoke(x, y),
                gesture => CameraGesture?.Invoke(gesture),
                shortcut => CameraShortcut?.Invoke(shortcut));
            Console.WriteLine($"[XYE] X11 视口已创建：0x{Xid.ToInt64():X}");
            XidCreated?.Invoke(Xid);
            return control;
        }
        catch
        {
            _mouse.Detach();
            base.DestroyNativeControlCore(control);
            Xid = 0;
            throw;
        }
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        _mouse.Detach();
        XidDestroying?.Invoke();
        base.DestroyNativeControlCore(control);
        Xid = 0;
    }
}