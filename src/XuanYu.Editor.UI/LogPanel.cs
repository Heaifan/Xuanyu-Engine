
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed class LogPanel : Border
{
    private readonly LogBox _log;

    private readonly SelectableTextBlock _output = new()
    {
        Foreground = EditorTheme.TextSub,
        FontFamily = new FontFamily("Consolas"),
        FontSize = 12,
        TextWrapping = TextWrapping.NoWrap
    };

    private readonly CheckBox _follow = new()
    {
        Content = "自动滚动",
        IsChecked = true,
        FontSize = 11,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 8, 0)
    };

    private readonly ScrollViewer _scroll;

    public LogPanel(LogBox log)
    {
        _log = log;
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);

        _scroll = new ScrollViewer
        {
            Content = _output,
            Background = EditorTheme.PanelBg,
            Padding = new Thickness(10, 6)
        };

        var header = new Grid
        {
            Background = EditorTheme.PanelHeaderBg,
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        header.Children.Add(new TextBlock
        {
            Text = "输出 / 日志",
            Foreground = EditorTheme.TextMain,
            FontWeight = FontWeight.SemiBold,
            FontSize = 11,
            Margin = new Thickness(10, 0),
            VerticalAlignment = VerticalAlignment.Center
        });

        Grid.SetColumn(_follow, 1);
        header.Children.Add(_follow);

        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("24,*")
        };

        Grid.SetRow(_scroll, 1);
        root.Children.Add(header);
        root.Children.Add(_scroll);

        Child = root;
        _log.Updated += Refresh;
        Refresh();
    }

    public void Refresh()
    {
        _output.Text = string.Join(Environment.NewLine, _log.Items);

        if (_follow.IsChecked == true)
        {
            Dispatcher.UIThread.Post(
                () => _scroll.ScrollToEnd(),
                DispatcherPriority.Loaded);
        }
    }
}
