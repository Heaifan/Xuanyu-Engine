using Avalonia;
using Avalonia.Controls;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

/// <summary>
/// 玄域引擎主工作台窗口。
/// </summary>
public sealed class EditorWindow : Window
{
    private readonly Workbench _wb;
    private readonly TopBar _topBar;
    private readonly SideCard _leftPanel = new("LEFT PANEL / 模块面板");
    private readonly SideCard _rightPanel = new("INSPECTOR / 属性检查器");
    private readonly ViewportBox _viewport;

    public EditorWindow(Workbench wb)
    {
        _wb = wb;
        Title = "XuanYu Engine Workbench (E0)";
        Width = 1280;
        Height = 760;
        Background = EditorTheme.WindowBg;

        _topBar = new TopBar(_wb);
        _viewport = new ViewportBox(_wb.ViewportNote);
        var logPanel = new LogPanel(_wb.Log);

        var centerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("260,*,300"),
            Margin = new Thickness(8)
        };
        Grid.SetColumn(_leftPanel, 0);
        Grid.SetColumn(_viewport, 1);
        Grid.SetColumn(_rightPanel, 2);
        centerGrid.Children.Add(_leftPanel);
        centerGrid.Children.Add(_viewport);
        centerGrid.Children.Add(_rightPanel);

        var rootGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,150")
        };
        Grid.SetRow(_topBar, 0);
        Grid.SetRow(centerGrid, 1);
        Grid.SetRow(logPanel, 2);
        rootGrid.Children.Add(_topBar);
        rootGrid.Children.Add(centerGrid);
        rootGrid.Children.Add(logPanel);

        Content = rootGrid;
        _wb.Changed += SyncUI;
        SyncUI();
    }

    private void SyncUI()
    {
        PageInfo info = _wb.Current;
        _topBar.Refresh();
        _leftPanel.SetText(info.LeftTitle, info.LeftDetail);
        _rightPanel.SetText(info.RightTitle, info.RightDetail);
        _viewport.UpdateOverlay(info.Title);
    }
}