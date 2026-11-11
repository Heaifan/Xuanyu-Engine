using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Right;

public sealed partial class RightPropertyGroup
{
    public void AddSubheading(string title)
    {
        _rows.Children.Add(new TextBlock
        {
            Text = title, FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = EditorTheme.TextSub,
            Margin = new Thickness(10, 6, 0, 2)
        });
    }

    public void AddText(string name, string value)
    {
        var grid = CreateRow(name);
        var text = new TextBlock
        {
            Text = value,
            FontSize = 12,
            Foreground = EditorTheme.TextMain,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(6, 0, 0, 0)
        };
        ToolTip.SetTip(text, value);
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);
        _rows.Children.Add(grid);
    }

    public void AddVisibility(bool visible, Action<bool> onChanged)
    {
        var grid = CreateRow("视口可见");
        grid.Height = 28;
        var check = new CheckBox
        {
            IsChecked = visible,
            MinHeight = 24,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 4, 0)
        };
        check.Click += (_, _) => onChanged(check.IsChecked == true);
        Grid.SetColumn(check, 1);
        grid.Children.Add(check);
        _rows.Children.Add(grid);
    }

    public void AddNumber(string name, float value,
        Action<float> onChanged, float step = 0.1f) =>
        _rows.Children.Add(new RightNumberField(name, value, step, onChanged));

    private static Grid CreateRow(string name)
    {
        var grid = new Grid
        {
            Height = 24,
            Margin = new Thickness(8, 0),
            ColumnDefinitions = new ColumnDefinitions("2*,3*")
        };
        grid.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 12,
            Foreground = EditorTheme.TextSub,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        return grid;
    }
}