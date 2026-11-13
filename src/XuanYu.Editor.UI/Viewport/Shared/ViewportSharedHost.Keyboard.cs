using Avalonia.Input;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private void OnViewportKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Home)
        {
            OnCameraShortcut(ViewportCameraShortcut.Home);
            e.Handled = true;
        }
        else if (e.Key == Key.F && !_wb.IsSimulating)
        {
            OnCameraShortcut(ViewportCameraShortcut.FocusSelection);
            e.Handled = true;
        }
        else
        {
            GameCameraKey key = MapGameKey(e.Key);
            if (key == GameCameraKey.None) return;
            _wb.SetGameCameraKey(key, true);
            e.Handled = _wb.IsSimulating;
        }
    }

    private void OnViewportKeyUp(object? sender, KeyEventArgs e)
    {
        var key = MapGameKey(e.Key);
        if (key == GameCameraKey.None) return;
        _wb.SetGameCameraKey(key, false);
        e.Handled = _wb.IsSimulating;
    }

    private static GameCameraKey MapGameKey(Key key) => key switch
    {
        Key.W => GameCameraKey.Forward,
        Key.S => GameCameraKey.Backward,
        Key.A => GameCameraKey.Left,
        Key.D => GameCameraKey.Right,
        _ => GameCameraKey.None
    };
}