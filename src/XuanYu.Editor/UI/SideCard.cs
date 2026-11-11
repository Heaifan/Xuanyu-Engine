
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
    private readonly StackPanel _content = new()
    {
        Spacing = 9, Margin = new Thickness(12, 12, 12, 0)
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

        var footer = new Border
        {
            Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(10, 0),
            Child = new TextBlock
            {
                Text = "E1 · 预览工作台",
                FontSize = 10,
                Foreground = EditorTheme.TextMuted,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("26,*,22")
        };

        Grid.SetRow(_content, 1);
        Grid.SetRow(footer, 2);
        grid.Children.Add(header);
        grid.Children.Add(_content);
        grid.Children.Add(footer);
        Child = grid;
    }

    public void SetContent(Control content)
    {
        if (_content.Children.Count == 1 &&
            ReferenceEquals(_content.Children[0], content))
            return;

        _content.Children.Clear();
        _content.Children.Add(content);
    }

    public void SetText(string title, string detail)
    {
        _content.Children.Clear();
        _head.Text = title;
        _body.Text = detail;
        _content.Children.Add(_head);
        _content.Children.Add(_body);
    }
}
