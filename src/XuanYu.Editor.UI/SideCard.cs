using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

public sealed class SideCard : Border
{
    private readonly TextBlock _headerLabel;
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
        _headerLabel = WorkspacePanelHeader.CreateLabel(roleTag);
        var header = WorkspacePanelHeader.Create(_headerLabel);
        var grid = new Grid { RowDefinitions = new RowDefinitions("26,*") };
        Grid.SetRow(_content, 1);
        grid.Children.Add(header);
        grid.Children.Add(_content);
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
        _headerLabel.Text = title;
        _body.Text = detail;
        SetContent(_body);
    }
}