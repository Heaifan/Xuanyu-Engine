using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

/// <summary>
/// 顶部工作区导航与工具栏。
/// </summary>
public sealed class TopBar : Border
{
    private readonly Workbench _wb;
    private readonly Dictionary<PageId, Button> _buttons = [];

    public TopBar(Workbench wb)
    {
        _wb = wb;
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(0, 0, 0, 1);
        Padding = new Thickness(12, 6);

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center
        };

        row.Children.Add(new TextBlock
        {
            Text = "XuanYu Engine",
            Foreground = EditorTheme.TextMain,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 16, 0)
        });

        foreach (PageInfo page in PageCatalog.All)
        {
            var btn = new Button
            {
                Content = page.Title,
                Foreground = EditorTheme.TextMain,
                Padding = new Thickness(12, 6)
            };
            PageId targetId = page.Id;
            btn.Click += (_, _) => _wb.SwitchTo(targetId);
            _buttons[targetId] = btn;
            row.Children.Add(btn);
        }

        Child = row;
        Refresh();
    }

    public void Refresh()
    {
        foreach ((PageId id, Button btn) in _buttons)
        {
            btn.Background = id == _wb.CurrentId
                ? EditorTheme.Accent
                : EditorTheme.TabIdle;
        }
    }
}