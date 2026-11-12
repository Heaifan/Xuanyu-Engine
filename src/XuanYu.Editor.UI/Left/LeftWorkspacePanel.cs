using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
namespace XuanYu.Editor.UI.Left;
internal sealed class LeftWorkspacePanel : Border
{
    private static readonly FontFamily TreeFont = new("Microsoft YaHei UI");
    private readonly LeftSceneTree _tree;
    private readonly ScrollViewer _scroll = new();
    private readonly ContextMenu _sceneMenu;
    private readonly Grid _tabs;
    private readonly StackPanel _infoPanel = new() { Spacing = 9,
        Margin = new Thickness(4, 12, 4, 0) };
    private readonly TextBlock _infoTitle = new() { FontSize = 13,
        FontFamily = TreeFont, FontWeight = FontWeight.SemiBold, Foreground = EditorTheme.TextMain };
    private readonly TextBlock _infoDetail = new() { FontSize = 12,
        FontFamily = TreeFont, TextWrapping = TextWrapping.Wrap, Foreground = EditorTheme.TextSub };
    public LeftWorkspacePanel(PreviewScene scene)
    {
        _tree = new LeftSceneTree(scene);
        _sceneMenu = _tree.CreateSceneMenu();
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(3);
        _tabs = BuildTabs();
        _scroll.Padding = new Thickness(8, 0, 8, 0);
        _scroll.Background = Brushes.Transparent;
        _scroll.ContextMenu = _sceneMenu;
        _scroll.Content = _tree;
        _infoPanel.Children.Add(_infoTitle);
        _infoPanel.Children.Add(_infoDetail);
        var root = new Grid { RowDefinitions = new("26,Auto,*,22") };
        var header = new Border { Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 0),
            Child = new TextBlock { Text = "LEFT / 场景树", FontSize = 10,
                FontFamily = TreeFont,
                Foreground = EditorTheme.TextMuted,
                VerticalAlignment = VerticalAlignment.Center } };
        var footer = new Border { Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(10, 0),
            Child = new TextBlock { Text = "E1 · 预览场景", FontSize = 10,
                FontFamily = TreeFont,
                Foreground = EditorTheme.TextMuted,
                VerticalAlignment = VerticalAlignment.Center } };
        Grid.SetRow(_tabs, 1);
        Grid.SetRow(_scroll, 2);
        Grid.SetRow(footer, 3);
        root.Children.Add(header);
        root.Children.Add(_tabs);
        root.Children.Add(_scroll);
        root.Children.Add(footer);
        Child = root;
    }
    public void SetPage(PageInfo info, bool isWorld)
    {
        _tabs.IsVisible = isWorld;
        _scroll.ContextMenu = isWorld ? _sceneMenu : null;
        _infoTitle.Text = info.LeftTitle;
        _infoDetail.Text = info.LeftDetail;
        if ((_scroll.Content is LeftSceneTree) == isWorld)
        {
            if (isWorld) _tree.Refresh();
            return;
        }
        if (isWorld)
            _scroll.Content = _tree;
        else
            _scroll.Content = _infoPanel;
    }

    private static Grid BuildTabs()
    {
        var grid = new Grid { ColumnDefinitions = new("68,68,*") };
        var project = new Border { Height = 34,
            BorderBrush = EditorTheme.Accent,
            BorderThickness = new Thickness(0, 0, 0, 2),
            Child = new TextBlock { Text = "项目", FontSize = 12,
                FontFamily = TreeFont, FontWeight = FontWeight.SemiBold,
                Foreground = EditorTheme.Accent,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center } };
        var files = new TextBlock { Text = "文件", FontSize = 12,
            FontFamily = TreeFont, Foreground = EditorTheme.TextMuted,
            Opacity = 0.6, VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center };
        ToolTip.SetTip(files, "尚未接入项目目录，文件页暂不可用");
        Grid.SetColumn(files, 1);
        grid.Children.Add(project);
        grid.Children.Add(files);
        return grid;
    }
}