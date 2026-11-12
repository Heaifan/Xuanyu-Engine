using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed class TopBar : Border
{
    private readonly Workbench _wb;
    private readonly TopPageTabs _pages;
    private readonly TextBlock _status = new()
    {
        Foreground = EditorTheme.TextMuted,
        FontSize = 11,
        VerticalAlignment = VerticalAlignment.Center
    };
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
        Grid.SetColumn(_pages, 1);
        Grid.SetColumn(_status, 2);
        first.Children.Add(menu);
        first.Children.Add(_pages);
        first.Children.Add(_status);

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
        _status.Text = _wb.IsSimulating
            ? $"XYE / PLAY {_wb.Simulation.ElapsedSeconds:0.0}s"
            : "XYE / E3";
    }
}