using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Editor.Preview;
namespace XuanYu.Editor.UI.Left;
internal sealed partial class LeftWorkspacePanel : Border
{
    private static readonly FontFamily TreeFont = new("Microsoft YaHei UI");
    private readonly LeftSceneTree _tree;
    private readonly ScrollViewer _scroll = new();
    private readonly ContextMenu _sceneMenu;
    private readonly TextBlock _headerLabel = WorkspacePanelHeader.CreateLabel("场景树");
    private readonly StackPanel _infoPanel = new()
    { Spacing = 9, Margin = new Thickness(4, 12, 4, 0) };
    private readonly TextBlock _infoDetail = new()
    { FontSize = 12, FontFamily = TreeFont,
      TextWrapping = TextWrapping.Wrap, Foreground = EditorTheme.TextSub };
    public LeftWorkspacePanel(PreviewScene scene, Action? createCube = null)
    {
        _tree = new LeftSceneTree(scene, createCube);
        _sceneMenu = _tree.CreateSceneMenu();
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(3);
        _scroll.Padding = new Thickness(4, 0, 6, 0);
        _scroll.Background = Brushes.Transparent;
        _scroll.ContextMenu = _sceneMenu;
        _scroll.Content = _tree;
        _infoPanel.Children.Add(_infoDetail);
        var root = new Grid { RowDefinitions = new("26,*") };
        var header = WorkspacePanelHeader.Create(_headerLabel);
        Grid.SetRow(_scroll, 1);
        root.Children.Add(header);
        root.Children.Add(_scroll);
        Child = root;
    }
}