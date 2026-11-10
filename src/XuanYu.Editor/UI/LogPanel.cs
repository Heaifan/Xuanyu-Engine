using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

/// <summary>
/// 底部诊断与日志面板。
/// </summary>
public sealed class LogPanel : Border
{
    private readonly LogBox _log;
    private readonly ListBox _list = new()
    {
        Background = Brushes.Transparent,
        Foreground = EditorTheme.TextSub
    };

    public LogPanel(LogBox log)
    {
        _log = log;
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(0, 1, 0, 0);
        Padding = new Thickness(10, 6);
        Child = _list;

        _log.Updated += Refresh;
        Refresh();
    }

    public void Refresh()
    {
        _list.ItemsSource = _log.Items.ToArray();
        if (_log.Items.Count > 0)
        {
            _list.SelectedIndex = _log.Items.Count - 1;
        }
    }
}