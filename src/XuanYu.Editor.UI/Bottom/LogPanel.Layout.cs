using Avalonia;
using Avalonia.Input.Platform;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

public sealed partial class LogPanel
{
    private Control BuildLayout()
    {
        var root = new Grid { RowDefinitions = new("26,*") };
        var header = new Grid
        {
            Background = EditorTheme.PanelHeaderBg,
            ColumnDefinitions = new("Auto,*,Auto")
        };
        var tabs = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        foreach (string name in new[] { "日志", "问题", "输出", "性能", "调试" })
        {
            var button = new Button
            {
                Content = name, Padding = new Thickness(12, 4),
                FontSize = 11, Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            button.Click += (_, _) => SelectTab(name);
            var underline = new Border
            {
                Child = button, BorderThickness = new Thickness(0, 0, 0, 2),
                BorderBrush = Brushes.Transparent
            };
            _tabs.Add(name, underline);
            tabs.Children.Add(underline);
        }
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal, Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        _level.ItemsSource = new[] { "全部", "信息", "警告", "错误", "调试" };
        _level.SelectedIndex = 0;
        _search.TextChanged += (_, _) => RebuildRows();
        _level.SelectionChanged += (_, _) => RebuildRows();
        var copy = new Button { Content = "复制", FontSize = 11,
            Padding = new Thickness(7, 2) };
        copy.Click += async (_, _) =>
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
                await clipboard.SetTextAsync(string.Join(Environment.NewLine, _rows));
        };
        _fold.Click += (_, _) => ToggleFold();
        actions.Children.Add(_search);
        actions.Children.Add(_level);
        actions.Children.Add(copy);
        actions.Children.Add(_follow);
        actions.Children.Add(_fold);
        header.Children.Add(tabs);
        Grid.SetColumn(actions, 2);
        header.Children.Add(actions);
        Grid.SetRow(_body, 1);
        root.Children.Add(header);
        root.Children.Add(_body);
        return root;
    }

    private void ToggleFold()
    {
        _folded = !_folded;
        _fold.Content = _folded ? "展开" : "收起";
        _body.IsVisible = !_folded;
        FoldChanged?.Invoke(_folded);
    }
}