
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Right;

public sealed class RightObjectHeader : Border
{
    public RightObjectHeader(
        RightInspectorSnapshot info, Func<string, int, Control>? iconFactory = null)
    {
        Height = 40;
        Padding = new Thickness(8, 3);
        var glyph = iconFactory?.Invoke(info.IconKey, 12) ?? MakeFallbackIcon();
        glyph.HorizontalAlignment = HorizontalAlignment.Center;
        glyph.VerticalAlignment = VerticalAlignment.Center;
        var icon = new Border
        {
            Width = 28,
            Height = 28,
            Background = EditorTheme.AccentSoft,
            CornerRadius = new CornerRadius(4),
            Child = glyph
        };
        var details = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 1,
            Children =
            {
                Label(info.Name, EditorTheme.TextMain, 12, FontWeight.SemiBold),
                Label($"{info.ObjectType} · {info.Context}", EditorTheme.TextSub, 11, FontWeight.Normal)
            }
        };
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("28,*"),
            ColumnSpacing = 7
        };
        Grid.SetColumn(details, 1);
        grid.Children.Add(icon);
        grid.Children.Add(details);
        Child = grid;
    }

    private static TextBlock Label(string text, IBrush color, double size, FontWeight weight)
    {
        var label = new TextBlock
        {
            Text = text,
            Foreground = color,
            FontSize = size,
            FontWeight = weight,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        ToolTip.SetTip(label, text);
        return label;
    }

    private static Control MakeFallbackIcon() => new TextBlock
    {
        Text = "◇",
        FontSize = 19,
        Foreground = EditorTheme.Accent,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };
}
