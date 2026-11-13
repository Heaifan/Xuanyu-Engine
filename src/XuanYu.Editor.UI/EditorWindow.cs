
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
        Title = "XuanYu Engine · Editor";
        Width = 1280; Height = 800;
        MinWidth = 1100; MinHeight = 600;
        Background = EditorTheme.WindowBg;
        _topBar = new TopBar(_wb, this);
        _leftPanel = new LeftWorkspacePanel(_wb.Scene);
        _leftPanel.BindTerrain(_wb);
        _inspector = new RightPreviewInspector(_wb.Scene);
        Control viewport = CreateViewport(vk);
        var root = new Grid { RowDefinitions = new("74,*") };
        var workspace = BuildWorkspace(viewport);
        Grid.SetRow(workspace, 1);
        root.Children.Add(_topBar);
        root.Children.Add(workspace);
        Content = root;
        _wb.Changed += SyncUI;
        SyncUI();
    }
    private Grid BuildWorkspace(Control viewport)
    {
        var log = new LogPanel(_wb.Log);
        var workspace = new Grid
        {
            ColumnDefinitions = new("236,6,*,6,248"),
            RowDefinitions = new("*,6,132"),
            Margin = new Thickness(4)
        };
        workspace.ColumnDefinitions[0].MinWidth = 170; workspace.ColumnDefinitions[2].MinWidth = 320;
        workspace.ColumnDefinitions[4].MinWidth = 190;
        workspace.RowDefinitions[0].MinHeight = 160; workspace.RowDefinitions[2].MinHeight = 75;
        Grid.SetColumn(_leftPanel, 0); Grid.SetRowSpan(_leftPanel, 3);
        Grid.SetColumn(viewport, 2);
        Grid.SetColumn(_rightHost, 4);
        Grid.SetColumn(log, 2); Grid.SetRow(log, 2);
        Grid.SetColumnSpan(log, 3);
        var leftSplit = MakeSplitter(GridResizeDirection.Columns);
        var rightSplit = MakeSplitter(GridResizeDirection.Columns);
        var bottomSplit = MakeSplitter(GridResizeDirection.Rows);
        Grid.SetColumn(leftSplit, 1); Grid.SetRowSpan(leftSplit, 3);
        Grid.SetColumn(rightSplit, 3);
        Grid.SetColumn(bottomSplit, 2); Grid.SetRow(bottomSplit, 1);
        Grid.SetColumnSpan(bottomSplit, 3);
        workspace.Children.Add(_leftPanel);
        workspace.Children.Add(viewport);
        workspace.Children.Add(_rightHost);
        workspace.Children.Add(log);
        workspace.Children.Add(leftSplit);
        workspace.Children.Add(rightSplit);
        workspace.Children.Add(bottomSplit);
        return workspace;
    }
    private static GridSplitter MakeSplitter(GridResizeDirection direction) => new()
    {
        ResizeDirection = direction,
        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        ShowsPreview = true,
        Background = EditorTheme.Border
    };

    private void SyncUI()
    {
        PageInfo info = _wb.Current;
        _topBar.Refresh();
        bool isWorld = _wb.CurrentId == PageId.World;
        _leftPanel.SetPage(info, isWorld, _wb.CurrentId == PageId.Terrain);
        _leftPanel.IsEnabled = !_wb.IsSimulating;
        _inspector.IsEnabled = !_wb.IsSimulating;
        if (isWorld)
            _rightHost.Content = _inspector;
        else if (_wb.CurrentId == PageId.Terrain)
            SyncTerrainInspector();
        else
        {
            _otherRight.SetText(info.RightTitle, info.RightDetail);
            _rightHost.Content = _otherRight;
        }
        _nativeViewport?.UpdateOverlay(info.Title);
    }
}
