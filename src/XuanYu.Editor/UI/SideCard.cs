
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

public sealed class SideCard : Border
{
    private readonly TextBlock _head = new()
    {
        Foreground = EditorTheme.TextMain,
        FontWeight = FontWeight.SemiBold,
        FontSize = 13
    };

    private readonly TextBlock _body = new()
    {
        Foreground = EditorTheme.TextSub,
        TextWrapping = TextWrapping.Wrap,
        FontSize = 12,
        LineHeight = 18
    };

    public SideCard(string roleTag)
    {
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(3);

        var header = new Border
        {
            Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 0),
            Child = new TextBlock
            {
                Text = roleTag,
                Foreground = EditorTheme.TextMuted,
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var content = new StackPanel
        {
            Spacing = 9,
            Margin = new Thickness(12, 12, 12, 0),
            Children = { _head, _body }
        };

        var footer = new Border
        {
            Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(10, 0),
            Child = new TextBlock
            {
                Text = "E0 · 工作区框架",
                FontSize = 10,
                Foreground = EditorTheme.TextMuted,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("26,*,22")
        };

        Grid.SetRow(content, 1);
        Grid.SetRow(footer, 2);
        grid.Children.Add(header);
        grid.Children.Add(content);
        grid.Children.Add(footer);
        Child = grid;
    }

    public void SetText(string title, string detail)
    {
        _head.Text = title;
        _body.Text = detail;
    }
}
