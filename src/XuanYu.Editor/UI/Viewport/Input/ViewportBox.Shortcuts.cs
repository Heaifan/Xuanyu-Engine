namespace XuanYu.Editor.UI;

public sealed partial class ViewportBox
{
    internal event Action<ViewportCameraShortcut>? CameraShortcut
    {
        add
        {
            if (_windows is not null) _windows.CameraShortcut += value;
            if (_linux is not null) _linux.CameraShortcut += value;
        }
        remove
        {
            if (_windows is not null) _windows.CameraShortcut -= value;
            if (_linux is not null) _linux.CameraShortcut -= value;
        }
    }
}