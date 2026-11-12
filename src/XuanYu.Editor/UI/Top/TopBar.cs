using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed class TopBar : Border
{
    private readonly Workbench _wb;
    private readonly TopPageTabs _pages;
    private readonly TextBlock _context = new()
    {
        Foreground = EditorTheme.TextSub,
        FontSize = 11
    };

    public TopBar(Workbench wb, Window owner)
    {
        _wb = wb;
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(0, 0, 0, 1);

        var first = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Margin = new Thickness(6, 0)
        };
        var menu = TopMenuBar.Create(owner, wb);
        _pages = new TopPageTabs(wb);
        var status = new TextBlock
        {
            Text = "XYE / E0",
            Foreground = EditorTheme.TextMuted,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_pages, 1);
        Grid.SetColumn(status, 2);
        first.Children.Add(menu);
        first.Children.Add(_pages);
        first.Children.Add(status);

        var tools = TopToolRow.Create(_context);
        var root = new Grid { RowDefinitions = new RowDefinitions("38,36") };
        Grid.SetRow(tools, 1);
        root.Children.Add(first);
        root.Children.Add(tools);
        Child = root;
        Refresh();
    }

    public void Refresh()
    {
        _pages.Refresh();
        _context.Text = $"当前：{_wb.CurrentId}";
    }
}