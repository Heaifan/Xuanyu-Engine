using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed partial class TerrainWorkspaceToolbar : Grid
{
    private readonly Workbench _wb;
    private readonly ViewportSharedHost? _shared;
    private readonly StackPanel _tools = new() { Spacing = 4 };
    private readonly Border _rail;
    private readonly List<(TerrainToolGroup Group, Button Button, Func<bool> Selected)> _toolButtons = [];

    public TerrainWorkspaceToolbar(Workbench wb, Control viewport)
    {
        _wb = wb;
        _shared = viewport as ViewportSharedHost;
        ClipToBounds = true;
        BuildTools();
        _rail = new Border { Child = new ScrollViewer { Content = _tools,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto },
            Background = EditorTheme.PanelBg, Padding = new Thickness(4),
            BorderBrush = EditorTheme.Border, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5), HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(8, 48, 0, 54) };
        if (_shared is null)
        {
            // 原生子窗口不能可靠地叠放 Avalonia 工具控件。
            ColumnDefinitions = new("Auto,*");
            SetColumn(viewport, 1);
            _rail.Margin = new Thickness(0);
        }
        Children.Add(viewport);
        Children.Add(_rail);
        wb.Changed += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        bool visible = _wb.CanPreviewTerrainBrush;
        _rail.IsVisible = visible;
        _shared?.SetTerrainToolbarVisible(visible);
        var active = _wb.ActiveTerrainToolGroup;
        foreach (var (group, button, selected) in _toolButtons)
        {
            button.IsVisible = group == active;
            Paint(button, group == active && selected());
        }
    }

    private static void Paint(Button button, bool active)
    {
        button.Background = active ? EditorTheme.AccentSoft : EditorTheme.PanelBg;
        button.Foreground = active ? EditorTheme.Accent : EditorTheme.TextMain;
        if (button.Content is Avalonia.Controls.Shapes.Path icon)
            icon.Stroke = button.Foreground;
    }
}