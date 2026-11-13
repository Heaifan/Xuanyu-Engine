
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Preview;
namespace XuanYu.Editor.UI.Left;
internal sealed partial class LeftWorkspacePanel : Border
{
    private static readonly FontFamily TreeFont = new("Microsoft YaHei UI");
    private readonly LeftSceneTree _tree;
    private readonly ScrollViewer _scroll = new();
    private readonly ContextMenu _sceneMenu;
    private readonly Grid _tabs;
    private readonly StackPanel _infoPanel = new()
    { Spacing = 9, Margin = new Thickness(4, 12, 4, 0) };
    private readonly TextBlock _infoTitle = new()
    { FontSize = 13, FontFamily = TreeFont,
      FontWeight = FontWeight.SemiBold, Foreground = EditorTheme.TextMain };
    private readonly TextBlock _infoDetail = new()
    { FontSize = 12, FontFamily = TreeFont,
      TextWrapping = TextWrapping.Wrap, Foreground = EditorTheme.TextSub };
    private readonly TextBlock _footerLabel = new()
    { FontSize = 10, FontFamily = TreeFont,
      Foreground = EditorTheme.TextMuted,
      VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
    public LeftWorkspacePanel(PreviewScene scene)
    {
        _tree = new LeftSceneTree(scene);
        _sceneMenu = _tree.CreateSceneMenu();
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(3);
        _tabs = BuildTabs();
        _scroll.Padding = new Thickness(4, 0, 6, 0);
        _scroll.Background = Brushes.Transparent;
        _scroll.ContextMenu = _sceneMenu;
        _scroll.Content = _tree;
        _infoPanel.Children.Add(_infoTitle);
        _infoPanel.Children.Add(_infoDetail);
        var root = new Grid { RowDefinitions = new("26,Auto,*,22") };
        var header = new Border
        {
            Background = EditorTheme.PanelHeaderBg, BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 0),
            Child = new TextBlock
            {
                Text = "LEFT / 场景树", FontSize = 10,
                FontFamily = TreeFont, Foreground = EditorTheme.TextMuted,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };
        var footer = new Border
        {
            Background = EditorTheme.PanelHeaderBg, BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(10, 0), Child = _footerLabel
        };
        Grid.SetRow(_tabs, 1);
        Grid.SetRow(_scroll, 2);
        Grid.SetRow(footer, 3);
        root.Children.Add(header);
        root.Children.Add(_tabs);
        root.Children.Add(_scroll);
        root.Children.Add(footer);
        Child = root;
    }
    private static Grid BuildTabs()
    {
        var grid = new Grid { ColumnDefinitions = new("60,60,*") };
        var project = new Border
        {
            Height = 34, BorderBrush = EditorTheme.Accent,
            BorderThickness = new Thickness(0, 0, 0, 2),
            Child = new TextBlock
            {
                Text = "项目", FontSize = 12, FontFamily = TreeFont,
                FontWeight = FontWeight.SemiBold, Foreground = EditorTheme.Accent,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
        var files = new TextBlock
        {
            Text = "文件", FontSize = 12, FontFamily = TreeFont,
            Foreground = EditorTheme.TextMuted, Opacity = 0.6,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        ToolTip.SetTip(files, "尚未接入项目目录，文件页暂不可用");
        Grid.SetColumn(files, 1);
        grid.Children.Add(project);
        grid.Children.Add(files);
        return grid;
    }
}
