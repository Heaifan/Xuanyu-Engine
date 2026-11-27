using Avalonia;
using Avalonia.Controls;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI.Left;
using XuanYu.Editor.UI.Right;
using XuanYu.Render.Vulkan;
using XuanYu.Runtime;

namespace XuanYu.Editor.UI;

public sealed partial class EditorWindow : Window
{
    private readonly Workbench _wb;
    private readonly TopBar _topBar;
    private readonly EditorStatusBar _statusBar = new();
    private readonly LeftWorkspacePanel _leftPanel;
    private readonly ContentControl _rightHost = new();
    private readonly SideCard _otherRight = new("属性检查器");
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
        var root = new Grid { RowDefinitions = new("74,*,22") };
        var workspace = BuildWorkspace(viewport);
        Grid.SetRow(workspace, 1);
        Grid.SetRow(_statusBar, 2);
        root.Children.Add(_topBar);
        root.Children.Add(workspace);
        root.Children.Add(_statusBar);
        Content = root;
        _wb.Changed += SyncUI;
        SyncUI();
    }

    private void SyncUI()
    {
        PageInfo info = _wb.Current;
        _topBar.Refresh();
        _statusBar.Refresh(_wb);
        bool isWorld = info.Id == PageId.World;
        bool isTerrain = info.Id == PageId.Terrain;
        var (leftDetail, rightDetail) = WorkspaceDetails(info);
        _leftPanel.SetPage(info, leftDetail);
        _leftPanel.IsEnabled = !_wb.IsSimulating || (!isWorld && !isTerrain);
        _inspector.IsEnabled = !_wb.IsSimulating;
        if (isWorld) _rightHost.Content = _inspector;
        else if (isTerrain) SyncTerrainInspector();
        else
        {
            _otherRight.SetText(info.RightTitle, rightDetail);
            _rightHost.Content = _otherRight;
        }
        _nativeViewport?.UpdateOverlay(info.Title);
    }

    private (string Left, string Right) WorkspaceDetails(PageInfo info) => info.Id switch
    {
        PageId.Project => (
            $"当前编辑场景\n网格对象：{_wb.Scene.Objects.Count}\n\n项目目录尚未接入。",
            "未选中项目资产。\n\n可通过 TOP 打开或保存 .xyscene 场景；项目资产管理尚未开放。"),
        PageId.Simulation => (
            $"状态：{(_wb.IsSimulating ? "运行中" : "已停止")}\n" +
            $"目标频率：{SimulationSession.TicksPerSecond} Hz\n\n通过 TOP 开始或停止模拟。",
            $"累计 Tick：{_wb.Simulation.TickCount}\n" +
            $"模拟时间：{_wb.Simulation.ElapsedSeconds:0.00} s\n" +
            $"运行态实体：{_wb.Simulation.RunningWorld?.Entities.Count ?? 0}"),
        PageId.Tools => (
            $"运行平台：{(OperatingSystem.IsWindows() ? "Windows" : "Linux")}\n" +
            $".NET：{Environment.Version}\n\n引擎配置工具尚未开放。",
            $"{_wb.ViewportNote}\n\n详细性能与诊断工具尚未开放。"),
        _ => (info.LeftDetail, info.RightDetail)
    };
}