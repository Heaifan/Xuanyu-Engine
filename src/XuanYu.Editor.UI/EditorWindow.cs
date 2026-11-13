using Avalonia;
using Avalonia.Controls;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI.Left;
using XuanYu.Editor.UI.Right;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

public sealed partial class EditorWindow : Window
{
    private readonly Workbench _wb;
    private readonly TopBar _topBar;
    private readonly LeftWorkspacePanel _leftPanel;
    private readonly ContentControl _rightHost = new();
    private readonly SideCard _otherRight = new("INSPECTOR / 属性检查器");
    private readonly RightPreviewInspector _inspector;
    private ViewportBox? _nativeViewport;
    private ViewportSharedHost? _sharedViewport;

    public EditorWindow(Workbench wb, VkContext? vk = null)
    {
        _wb = wb;
        Title = "XuanYu Engine · Editor (E3)";
        Width = 1280; Height = 800;
        MinWidth = 1100; MinHeight = 600;
        Background = EditorTheme.WindowBg;
        _topBar = new TopBar(_wb, this);
        _leftPanel = new LeftWorkspacePanel(_wb.Scene);
        _inspector = new RightPreviewInspector(_wb.Scene);
        Control viewport = CreateViewport(vk);
        var logPanel = new LogPanel(_wb.Log);
        _leftPanel.Margin = new Thickness(0, 0, 4, 0);
        viewport.Margin = new Thickness(0, 0, 4, 4);
        _rightHost.Margin = new Thickness(0, 0, 0, 4);
        var workspace = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("236,*,248"),
            RowDefinitions = new RowDefinitions("*,132"),
            Margin = new Thickness(4)
        };
        Grid.SetColumn(_leftPanel, 0); Grid.SetRowSpan(_leftPanel, 2);
        Grid.SetColumn(viewport, 1);
        Grid.SetColumn(_rightHost, 2);
        Grid.SetColumn(logPanel, 1); Grid.SetRow(logPanel, 1);
        Grid.SetColumnSpan(logPanel, 2);
        workspace.Children.Add(_leftPanel);
        workspace.Children.Add(viewport);
        workspace.Children.Add(_rightHost);
        workspace.Children.Add(logPanel);
        var root = new Grid { RowDefinitions = new RowDefinitions("74,*") };
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
        bool isWorld = _wb.CurrentId == PageId.World;
        _leftPanel.SetPage(info, isWorld);
        _leftPanel.IsEnabled = !_wb.IsSimulating;
        _inspector.IsEnabled = !_wb.IsSimulating;
        if (isWorld)
            _rightHost.Content = _inspector;
        else
        {
            _otherRight.SetText(info.RightTitle, info.RightDetail);
            _rightHost.Content = _otherRight;
        }
        _nativeViewport?.UpdateOverlay(info.Title);
    }
}