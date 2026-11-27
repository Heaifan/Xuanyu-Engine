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
        Control viewport = new TerrainWorkspaceToolbar(_wb, CreateViewport(vk));
        var root = new Grid { RowDefinitions = new("74,*") };
        var workspace = BuildWorkspace(viewport);
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
        _leftPanel.SetPage(info, isWorld, _wb.CurrentId == PageId.Terrain);
        _leftPanel.IsEnabled = !_wb.IsSimulating;
        _inspector.IsEnabled = !_wb.IsSimulating;
        if (isWorld) _rightHost.Content = _inspector;
        else if (_wb.CurrentId == PageId.Terrain) SyncTerrainInspector();
        else
        {
            _otherRight.SetText(info.RightTitle, info.RightDetail);
            _rightHost.Content = _otherRight;
        }
        _nativeViewport?.UpdateOverlay(info.Title);
    }
}