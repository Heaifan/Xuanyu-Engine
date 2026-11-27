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
    private readonly StackPanel _groups = new() { Orientation = Orientation.Horizontal, Spacing = 2 };
    private readonly StackPanel _tools = new() { Spacing = 4 };
    private readonly Border _header, _rail;
    private readonly Dictionary<TerrainToolGroup, Button> _groupButtons = [];
    private readonly List<(TerrainToolGroup Group, Button Button, Func<bool> Selected)> _toolButtons = [];

    public TerrainWorkspaceToolbar(Workbench wb, Control viewport)
    {
        _wb = wb;
        _shared = viewport as ViewportSharedHost;
        ClipToBounds = true;
        BuildGroups();
        BuildTools();
        _header = new Border { Child = _groups, Background = EditorTheme.PanelBg,
            Padding = new Thickness(6, 3), BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(5),
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(8, 8, 8, 0) };
        _rail = new Border { Child = new ScrollViewer { Content = _tools,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto },
            Background = EditorTheme.PanelBg, Padding = new Thickness(4),
            BorderBrush = EditorTheme.Border, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5), HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(8, 48, 0, 54) };
        if (_shared is null) ConfigureNativeLayout(viewport);
        Children.Add(viewport);
        Children.Add(_header);
        Children.Add(_rail);
        wb.Changed += Refresh;
        Refresh();
    }

    private void ConfigureNativeLayout(Control viewport)
    {
        // 原生子窗口可能遮住 Avalonia 控件，保留 UX01 的兼容布局。
        RowDefinitions = new("Auto,*");
        ColumnDefinitions = new("Auto,*");
        SetColumnSpan(_header, 2);
        SetRow(_rail, 1);
        SetRow(viewport, 1); SetColumn(viewport, 1);
        _header.Margin = new Thickness(0);
        _rail.Margin = new Thickness(0);
    }

    private void Refresh()
    {
        bool visible = _wb.CanPreviewTerrainBrush;
        _header.IsVisible = visible;
        _rail.IsVisible = visible;
        _shared?.SetTerrainToolbarVisible(visible);
        var active = _wb.ActiveTerrainToolGroup;
        foreach (var (group, button) in _groupButtons)
            Paint(button, group == active);
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