using XuanYu.Editor.Camera;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportBox
{
    internal event Action<GameCameraKey, bool>? GameInput
    {
        add
        {
            if (_windows is not null) _windows.GameInput += value;
            if (_linux is not null) _linux.GameInput += value;
        }
        remove
        {
            if (_windows is not null) _windows.GameInput -= value;
            if (_linux is not null) _linux.GameInput -= value;
        }
    }
}