using Avalonia.Controls;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportNavigationPanel
{
    private bool _panMode;
    internal event Action<bool>? PanModeChanged;

    private void TogglePanMode()
    {
        if (!_panMode && Camera() is null) return;
        _panMode = !_panMode;
        _panButton.Background = _panMode
            ? new SolidColorBrush(Color.Parse("#C9DEF3"))
            : new SolidColorBrush(Color.Parse("#EFF3F7"), 0.88);
        ToolTip.SetTip(_panButton, _panMode
            ? "平移开启：视口左键拖动；再次点击退出"
            : "进入平移模式：视口左键拖动");
        Console.WriteLine($"[XYE] 视口平移模式：{(_panMode ? "开启" : "关闭")}");
        PanModeChanged?.Invoke(_panMode);
    }
}