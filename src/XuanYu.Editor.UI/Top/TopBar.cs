using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI.Top.Persistence;
using XuanYu.Editor.UI.Top.Simulation;

namespace XuanYu.Editor.UI;

public sealed class TopBar : Border
{
    private readonly TopPageTabs _pages;
    private readonly TextBlock _status = new()
    {
        Text = "XYE / EDITOR",
        Foreground = EditorTheme.TextMuted,
        FontSize = 11,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Right
    };
    private readonly TopToolRow _tools;

    public TopBar(Workbench wb, Window owner)
    {
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(0, 0, 0, 1);
        var first = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,*"),
            Margin = new Thickness(6, 0)
        };
        var scene = new SceneFileCommands(owner, wb);
        var simulation = new SimulationMenu(owner, wb);
        var menu = TopMenuBar.Create(owner, wb, scene, simulation);
        menu.HorizontalAlignment = HorizontalAlignment.Left;
        _pages = new TopPageTabs(wb);
        Grid.SetColumn(_pages, 1);
        Grid.SetColumn(_status, 2);
        first.Children.Add(menu);
        first.Children.Add(_pages);
        first.Children.Add(_status);
        _tools = new TopToolRow(wb, scene, simulation);
        var root = new Grid { RowDefinitions = new RowDefinitions("38,36") };
        Grid.SetRow(_tools, 1);
        root.Children.Add(first);
        root.Children.Add(_tools);
        Child = root;
        Refresh();
    }

    public void Refresh()
    {
        _pages.Refresh();
        _tools.Refresh();
    }
}