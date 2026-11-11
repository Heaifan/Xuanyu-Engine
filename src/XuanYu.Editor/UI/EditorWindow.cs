using Avalonia;
using Avalonia.Controls;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Editor.UI.Left;
using XuanYu.Editor.UI.Right;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

public sealed class EditorWindow : Window
{
    private readonly Workbench _wb;
    private readonly TopBar _topBar;
    private readonly LeftWorkspacePanel _leftPanel;
    private readonly ContentControl _rightHost = new();
    private readonly SideCard _otherRight = new("INSPECTOR / 属性检查器");
    private readonly RightPreviewInspector _inspector;
    private readonly ViewportBox _viewport;

    public EditorWindow(Workbench wb, VkContext? vk = null)
    {
        _wb = wb;
        Title = "XuanYu Engine · Editor (E1)";
        Width = 1280;
        Height = 800;
        MinWidth = 1100;
        MinHeight = 600;
        Background = EditorTheme.WindowBg;
        _topBar = new TopBar(_wb, this);
        _viewport = new ViewportBox(_wb.ViewportNote);
        _leftPanel = new LeftWorkspacePanel(_wb.Scene);
        _inspector = new RightPreviewInspector(_wb.Scene);
        _viewport.Clicked += OnViewportClicked;
        if (vk is not null)
        {
            _ = new ViewportSwapchainSession(_viewport, vk, _wb);
            Closing += (_, _) => vk.DetachWin32Surface();
        }
        var logPanel = new LogPanel(_wb.Log);
        _leftPanel.Margin = new Thickness(0, 0, 4, 0);
        _viewport.Margin = new Thickness(0, 0, 4, 4);
        _rightHost.Margin = new Thickness(0, 0, 0, 4);
        var workspace = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("236,*,248"),
            RowDefinitions = new RowDefinitions("*,132"),
            Margin = new Thickness(4)
        };
        Grid.SetColumn(_leftPanel, 0); Grid.SetRowSpan(_leftPanel, 2);
        Grid.SetColumn(_viewport, 1);
        Grid.SetColumn(_rightHost, 2);
        Grid.SetColumn(logPanel, 1); Grid.SetRow(logPanel, 1);
        Grid.SetColumnSpan(logPanel, 2);
        workspace.Children.Add(_leftPanel);
        workspace.Children.Add(_viewport);
        workspace.Children.Add(_rightHost);
        workspace.Children.Add(logPanel);
        var root = new Grid { RowDefinitions = new RowDefinitions("74,*") };
        Grid.SetRow(workspace, 1);
        root.Children.Add(_topBar); root.Children.Add(workspace);
        Content = root;
        _wb.Changed += SyncUI;
        SyncUI();
    }

    private void OnViewportClicked(int x, int y)
    {
        if (_wb.CurrentId != PageId.World)
            return;
        var (width, height) = _viewport.PixelSize;
        Guid? hit = PreviewMeshPicker.Pick(_wb.Scene, x, y, width, height);
        if (_wb.Scene.SelectedId == hit)
            return;
        _wb.Scene.Select(hit);
        _wb.Log.Write(hit is null
            ? "视口点击空白，已取消选择"
            : $"视口选中：{_wb.Scene.Selected!.Name}");
    }

    private void SyncUI()
    {
        PageInfo info = _wb.Current;
        _topBar.Refresh();
        bool isWorld = _wb.CurrentId == PageId.World;
        _leftPanel.SetPage(info, isWorld);
        if (isWorld)
            _rightHost.Content = _inspector;
        else
        {
            _otherRight.SetText(info.RightTitle, info.RightDetail);
            _rightHost.Content = _otherRight;
        }
        _viewport.UpdateOverlay(info.Title);
    }
}