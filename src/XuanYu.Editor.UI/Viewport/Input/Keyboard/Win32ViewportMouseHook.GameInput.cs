using XuanYu.Editor.Camera;

namespace XuanYu.Editor.UI;

internal sealed partial class Win32ViewportMouseHook
{
    private Action<GameCameraKey, bool>? _gameInput;

    private bool DispatchGameInput(nuint code, bool down)
    {
        GameCameraKey key = GameCameraKeyMap.FromKeySymbol((uint)code);
        if (key == GameCameraKey.None)
            return false;
        _gameInput?.Invoke(key, down);
        return true;
    }
}