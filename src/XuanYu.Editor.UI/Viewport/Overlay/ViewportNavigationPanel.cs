using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportNavigationPanel : StackPanel
{
    private readonly ViewportAxisGizmo _axis = new();
    private readonly Button _projection = MakeButton("projection", "切换透视/正交");
    private readonly Button _panButton = MakeButton("pan", "进入平移模式");
    private Workbench? _workbench;

    public ViewportNavigationPanel()
    {
        Width = 76; Spacing = 2;
        Children.Add(_axis);
        var zoom = MakeButton("zoom", "点击放大；滚轮双向缩放");
        zoom.Click += (_, _) => Camera()?.Zoom(1);
        zoom.PointerWheelChanged += (_, e) =>
        {
            Camera()?.Zoom((float)e.Delta.Y);
            e.Handled = true;
        };
        Children.Add(zoom);

        _panButton.Click += (_, _) => TogglePanMode();
        Children.Add(_panButton);
        var sceneCamera = MakeButton("camera", "暂无 Camera Entity，尚未开放");
        sceneCamera.IsEnabled = false;
        ToolTip.SetShowOnDisabled(sceneCamera, true);
        Children.Add(sceneCamera);

        _projection.Click += (_, _) =>
        {
            if (Camera() is not { } camera) return;
            camera.ToggleProjection();
            ToolTip.SetTip(_projection, camera.IsOrthographic
                ? "当前正交投影；点击切换透视"
                : "当前透视投影；点击切换正交");
        };
        Children.Add(_projection);
    }

    public void Bind(Workbench workbench)
    {
        _workbench = workbench;
        _axis.Bind(Camera);
    }

    private ViewportCamera? Camera() =>
        _workbench is { CurrentId: PageId.World } wb
            ? wb.Scene.Camera : null;

    private static Button MakeButton(string name, string tip)
    {
        var button = new Button
        {
            Content = ViewportNavIcons.Create(name), Width = 29, Height = 29,
            HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 5, 0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Padding = new Thickness(0), CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.Parse("#EFF3F7"), 0.88),
            BorderThickness = new Thickness(0)
        };
        ToolTip.SetTip(button, tip);
        return button;
    }
}