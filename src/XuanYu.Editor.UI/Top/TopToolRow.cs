
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace XuanYu.Editor.UI;

internal static class TopToolRow
{
    public static Border Create(TextBlock context)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(
                "Auto,Auto,Auto,*,Auto,120")
        };

        var edit = Group("编辑", "选择", "移动", "旋转", "缩放");
        var view = Group("视图", "聚焦", "网格");
        var snap = Group("吸附", "关闭");

        var search = new TextBox
        {
            PlaceholderText = "搜索场景/资产（待接入）",
            IsEnabled = false,
            Width = 180,
            Height = 27,
            FontSize = 11,
            Margin = new Thickness(0, 0, 12, 0)
        };

        context.VerticalAlignment = VerticalAlignment.Center;
        context.HorizontalAlignment = HorizontalAlignment.Right;
        context.Margin = new Thickness(0, 0, 6, 0);

        Grid.SetColumn(edit, 0);
        Grid.SetColumn(view, 1);
        Grid.SetColumn(snap, 2);
        Grid.SetColumn(search, 4);
        Grid.SetColumn(context, 5);

        row.Children.Add(edit);
        row.Children.Add(view);
        row.Children.Add(snap);
        row.Children.Add(search);
        row.Children.Add(context);

        return new Border
        {
            Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(8, 3),
            Child = row
        };
    }

    private static StackPanel Group(
        string title, params string[] commands)
    {
        var group = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 3,
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        group.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = EditorTheme.TextSub,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0)
        });

        foreach (string command in commands)
        {
            group.Children.Add(new Button
            {
                Content = command,
                IsEnabled = false,
                FontSize = 11,
                Padding = new Thickness(7, 3)
            });
        }

        return group;
    }
}
