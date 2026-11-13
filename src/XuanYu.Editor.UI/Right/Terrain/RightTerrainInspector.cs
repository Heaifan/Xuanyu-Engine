using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector : Border
{
    private readonly Workbench _wb;
    private readonly StackPanel _rows = new()
    {
        Spacing = 2, Margin = new Thickness(9, 5)
    };
    private bool _showMapInfo = true;
    private bool _showHeightField = true;
    private bool _showSource = true;

    public RightTerrainInspector(Workbench wb)
    {
        _wb = wb;
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(3);

        var root = new Grid { RowDefinitions = new("*,22") };
        var footer = new Border
        {
            Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(10, 0),
            Child = new TextBlock
            {
                Text = "地形数据",
                FontSize = 10,
                Foreground = EditorTheme.TextMuted,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        var scroll = new ScrollViewer { Content = _rows };
        Grid.SetRow(footer, 1);
        root.Children.Add(scroll);
        root.Children.Add(footer);
        Child = root;
    }
}