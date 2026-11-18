using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportNavigationPanel
{
    private bool _panMode;
    private bool _handDown;
    private bool _handDragged;
    private Point _handStart;
    private Point _handLast;
    internal event Action<bool>? PanModeChanged;

    private void InitializePanButton()
    {
        _panButton.Click += (_, _) =>
        {
            if (!_handDragged) TogglePanMode();
        };
        _panButton.AddHandler(InputElement.PointerPressedEvent,
            HandPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        _panButton.AddHandler(InputElement.PointerMovedEvent,
            HandMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        _panButton.AddHandler(InputElement.PointerReleasedEvent,
            HandReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        _panButton.PointerCaptureLost += (_, _) => _handDown = false;
    }

    private void HandPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(_panButton).Properties.IsLeftButtonPressed) return;
        _handDown = true;
        _handDragged = false;
        _handStart = _handLast = e.GetPosition(_panButton);
    }

    private void HandMoved(object? sender, PointerEventArgs e)
    {
        if (!_handDown) return;
        Point next = e.GetPosition(_panButton);
        if (!_handDragged)
        {
            double moved = Math.Abs(next.X - _handStart.X) +
                Math.Abs(next.Y - _handStart.Y);
            if (moved < 3d) return;
            _handDragged = true;
        }
        double scale = TopLevel.GetTopLevel(_panButton)?.RenderScaling ?? 1d;
        PanActive((float)((next.X - _handLast.X) * scale),
            (float)((next.Y - _handLast.Y) * scale));
        _handLast = next;
    }

    private void HandReleased(object? sender, PointerReleasedEventArgs e) =>
        _handDown = false;

    private void TogglePanMode()
    {
        if (!_panMode && Camera() is null && GeoCamera() is null) return;
        _panMode = !_panMode;
        _panButton.Background = _panMode
            ? new SolidColorBrush(Color.Parse("#C9DEF3"))
            : new SolidColorBrush(Color.Parse("#EFF3F7"), 0.88);
        ToolTip.SetTip(_panButton, _panMode
            ? "平移模式开启：视口左键拖动；点击手掌退出"
            : "按住手掌拖动平移；点击切换左键平移模式");
        Console.WriteLine($"[XYE] 视口平移模式：{(_panMode ? "开启" : "关闭")}");
        PanModeChanged?.Invoke(_panMode);
    }
}