using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private void OnViewportPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this)) return;
        var properties = e.GetCurrentPoint(this).Properties;
        bool middle = properties.IsMiddleButtonPressed;
        bool left = properties.IsLeftButtonPressed;
        if (middle || (_panMode && left))
        {
            _dragging = true;
            _panGesture = !middle || e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            _previous = e.GetPosition(this);
            e.Pointer.Capture(this);
            Focus();
            e.Handled = true;
        }
        else if (left)
        {
            Focus();
            Point point = e.GetPosition(this);
            double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
            Clicked?.Invoke((int)Math.Round(point.X * scale),
                (int)Math.Round(point.Y * scale));
            e.Handled = true;
        }
    }

    private void OnViewportPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging) return;
        Point next = e.GetPosition(this);
        double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        OnCameraGesture(new ViewportCameraGesture(
            (float)((next.X - _previous.X) * scale),
            (float)((next.Y - _previous.Y) * scale), 0, _panGesture));
        _previous = next;
        e.Handled = true;
    }

    private void OnViewportPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void OnViewportWheel(object? sender, PointerWheelEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this)) return;
        OnCameraGesture(new ViewportCameraGesture(0, 0, (float)e.Delta.Y, false));
        e.Handled = true;
    }
}