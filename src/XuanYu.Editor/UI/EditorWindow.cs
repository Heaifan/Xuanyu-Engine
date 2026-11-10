
using Avalonia;
using Avalonia.Controls;
using XuanYu.Editor.Pages;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

public sealed class EditorWindow : Window
{
    private readonly Workbench _wb;
    private readonly TopBar _topBar;
    private readonly SideCard _leftPanel = new("LEFT / 模块面板");
    private readonly SideCard _rightPanel = new("INSPECTOR / 属性检查器");
    private readonly ViewportBox _viewport;

    public EditorWindow(Workbench wb, VkContext? vk = null)
    {
        _wb = wb;
        Title = "XuanYu Engine · Editor (E0)";
        Width = 1280;
        Height = 800;
        MinWidth = 1100;
        MinHeight = 600;
        Background = EditorTheme.WindowBg;

        _topBar = new TopBar(_wb, this);
        _viewport = new ViewportBox(_wb.ViewportNote);

        if (vk is not null)
        {
            _ = new ViewportSwapchainSession(_viewport, vk, _wb);
            Closing += (_, _) => vk.DetachWin32Surface();
        }

        var logPanel = new LogPanel(_wb.Log);

        _leftPanel.Margin = new Thickness(0, 0, 4, 0);
        _viewport.Margin = new Thickness(0, 0, 4, 4);
        _rightPanel.Margin = new Thickness(0, 0, 0, 4);

        var workspace = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("236,*,274"),
            RowDefinitions = new RowDefinitions("*,132"),
            Margin = new Thickness(4)
        };

        Grid.SetColumn(_leftPanel, 0);
        Grid.SetRowSpan(_leftPanel, 2);
        Grid.SetColumn(_viewport, 1);
        Grid.SetColumn(_rightPanel, 2);
        Grid.SetColumn(logPanel, 1);
        Grid.SetRow(logPanel, 1);
        Grid.SetColumnSpan(logPanel, 2);

        workspace.Children.Add(_leftPanel);
        workspace.Children.Add(_viewport);
        workspace.Children.Add(_rightPanel);
        workspace.Children.Add(logPanel);

        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("74,*")
        };

        Grid.SetRow(workspace, 1);
        root.Children.Add(_topBar);
        root.Children.Add(workspace);

        Content = root;
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
