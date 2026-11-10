
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed class TopPageTabs : StackPanel
{
    private readonly Workbench _wb;
    private readonly Dictionary<PageId, Button> _buttons = [];

    public TopPageTabs(Workbench wb)
    {
        _wb = wb;
        Orientation = Orientation.Horizontal;
        Spacing = 4;
        Margin = new Thickness(14, 0, 0, 0);
        VerticalAlignment = VerticalAlignment.Center;

        foreach (PageInfo page in PageCatalog.All)
        {
            string label = page.Id switch
            {
                PageId.Project => "项目",
                PageId.World => "世界",
                PageId.Terrain => "地形",
                PageId.Simulation => "模拟",
                _ => "工具"
            };
            var button = new Button
            {
                Content = label,
                FontSize = 12,
                Padding = new Thickness(10, 5),
                BorderThickness = new Thickness(0)
            };
            PageId id = page.Id;
            button.Click += (_, _) => _wb.SwitchTo(id);
            _buttons[id] = button;
            Children.Add(button);
        }
    }

    public void Refresh()
    {
        foreach ((PageId id, Button button) in _buttons)
        {
            bool active = id == _wb.CurrentId;
            button.Background = active ? EditorTheme.AccentSoft : EditorTheme.TabIdle;
            button.Foreground = active ? EditorTheme.Accent : EditorTheme.TextSub;
            button.FontWeight = active ? FontWeight.SemiBold : FontWeight.Normal;
        }
    }
}
