
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Right;

public sealed partial class RightPropertyGroup : StackPanel
{
    private readonly StackPanel _rows = new() { Spacing = 0 };
    private readonly ContentControl _arrow = new()
    {
        Width = 14,
        Height = 14,
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly Func<string, int, Control>? _iconFactory;

    public event Action<bool>? ExpandedChanged;

    public RightPropertyGroup(string title, Func<string, int, Control>? iconFactory = null)
    {
        _iconFactory = iconFactory;
        HorizontalAlignment = HorizontalAlignment.Stretch;

        var caption = new TextBlock
        {
            Text = title,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("14,*"),
            ColumnSpacing = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(caption, 1);
        heading.Children.Add(_arrow);
        heading.Children.Add(caption);

        var toggle = new Button
        {
            Height = 22,
            MinHeight = 0,
            Padding = new Thickness(8, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = EditorTheme.PanelHeaderBg,
            Foreground = EditorTheme.TextMain,
            BorderThickness = new Thickness(0),
            Content = heading
        };
        toggle.Click += (_, _) => SetExpanded(!_rows.IsVisible);
        Children.Add(toggle);
        Children.Add(_rows);
        SetExpanded(true);
    }

    public void SetExpanded(bool expanded)
    {
        _rows.IsVisible = expanded;
        var key = expanded ? "Controls/chevron-down" : "Controls/chevron-right";
        _arrow.Content = _iconFactory?.Invoke(key, 12) ?? new TextBlock
        {
            Text = expanded ? "▾" : "▸",
            FontSize = 12,
            Foreground = EditorTheme.TextSub
        };
        ExpandedChanged?.Invoke(expanded);
    }
}
