
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    private void AddGroup(string title, bool expanded, Action toggle)
    {
        var bar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        bar.Children.Add(expanded
            ? TerrainUiGlyphs.ChevronDown()
            : TerrainUiGlyphs.ChevronRight());
        bar.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = EditorTheme.TextMain,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });
        var row = new Border
        {
            Background = EditorTheme.PanelHeaderBg,
            Height = 28,
            Margin = new Thickness(0, 7, 0, 4),
            Padding = new Thickness(7, 0),
            Child = bar
        };
        row.PointerPressed += (_, e) =>
        {
            toggle();
            Refresh();
            e.Handled = true;
        };
        _rows.Children.Add(row);
    }

    private void AddPair(string name, string value)
    {
        var row = new Grid
        {
            ColumnDefinitions = new("85,*"),
            MinHeight = 26
        };
        var label = new TextBlock
        {
            Text = name,
            FontSize = 11,
            Foreground = EditorTheme.TextSub,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        var data = new TextBlock
        {
            Text = value,
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            Foreground = EditorTheme.TextMain,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(data, 1);
        row.Children.Add(label);
        row.Children.Add(data);
        _rows.Children.Add(row);
    }
}
