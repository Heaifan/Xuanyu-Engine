using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftTreeRow : Button
{
    private static readonly FontFamily TreeFont = new("Microsoft YaHei UI");
    private readonly TextBlock _title;
    private readonly Avalonia.Controls.Shapes.Path _icon;
    private readonly IBrush _normalIconStroke;
    private readonly FontWeight _normalWeight;
    private readonly LeftTreeNode _node;

    public LeftTreeRow(LeftTreeNode node, int depth,
        bool[] continuations, bool isLast)
    {
        _node = node;
        Height = 28;
        Padding = new Thickness(0);
        BorderThickness = new Thickness(2, 0, 0, 0);
        Background = Brushes.Transparent;
        BorderBrush = Brushes.Transparent;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        _normalWeight = node.Item is null
            ? depth == 0 ? FontWeight.Bold : FontWeight.SemiBold
            : FontWeight.Normal;
        _title = new TextBlock
        {
            Text = node.Title,
            FontFamily = TreeFont,
            FontSize = 12,
            FontWeight = _normalWeight,
            Foreground = EditorTheme.TextMain,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };
        var columns = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,17,21,*"),
            Children =
            {
                new LeftTreeGuide(depth, continuations, isLast)
            }
        };
        Control expander = node.Children.Count > 0
            ? LeftSvgIcon.Create(node.IsExpanded
                ? "Controls/chevron-down" : "Controls/chevron-right", 12)
            : new Border { Width = 12 };
        Grid.SetColumn(expander, 1);
        columns.Children.Add(expander);
        _icon = LeftSvgIcon.Create(node.Icon);
        _normalIconStroke = _icon.Stroke!;
        Grid.SetColumn(_icon, 2);
        columns.Children.Add(_icon);
        Grid.SetColumn(_title, 3);
        columns.Children.Add(_title);
        Content = columns;
    }

    public void Refresh(bool selected)
    {
        Background = selected ? EditorTheme.AccentSoft : Brushes.Transparent;
        BorderBrush = selected ? EditorTheme.Accent : Brushes.Transparent;
        _title.Foreground = selected ? EditorTheme.Accent : EditorTheme.TextMain;
        _title.FontWeight = selected ? FontWeight.SemiBold : _normalWeight;
        _title.Opacity = _node.Item?.Visible == false ? 0.55 : 1;
        _icon.Stroke = selected ? EditorTheme.Accent : _normalIconStroke;
    }
}