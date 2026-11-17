using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportAxisGizmo
{
    private int _pressedAxis = -2;
    private Point _pressedAt;
    private Point _previous;
    private bool _dragged;

    private void BeginOrbit(PointerPressedEventArgs e, int axis)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _pressedAxis = axis;
        _pressedAt = _previous = e.GetPosition(this);
        _dragged = false;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void MoveOrbit(PointerEventArgs e)
    {
        if (_pressedAxis == -2 || _pose?.Invoke() is null) return;
        Point next = e.GetPosition(this);
        if (!_dragged && Math.Abs(next.X - _pressedAt.X) +
            Math.Abs(next.Y - _pressedAt.Y) < 3) return;
        _dragged = true;
        _orbit?.Invoke((float)(next.X - _previous.X),
            (float)(next.Y - _previous.Y));
        _previous = next;
        Refresh();
    }

    private void EndOrbit(PointerReleasedEventArgs e)
    {
        if (_pressedAxis == -2) return;
        if (!_dragged && _pressedAxis >= 0)
            _snap?.Invoke(Axes[_pressedAxis]);
        _pressedAxis = -2;
        e.Pointer.Capture(null);
        Refresh();
    }

    private void Refresh()
    {
        if (_pose?.Invoke() is not { } pose) return;
        for (int i = 0; i < 6; i++)
        {
            double x = 38 + Vector3.Dot(Axes[i], pose.Right) * 27;
            double y = 43 - Vector3.Dot(Axes[i], pose.Up) * 27;
            bool front = Vector3.Dot(Axes[i], pose.Outward) > 0;
            _lines[i].StartPoint = new Point(38, 43);
            _lines[i].EndPoint = new Point(x, y);
            _lines[i].Opacity = front ? 0.95 : 0.45;
            Canvas.SetLeft(_dots[i], x - 10);
            Canvas.SetTop(_dots[i], y - 10);
            _dots[i].Opacity = front ? 1 : 0.65;
            _dots[i].ZIndex = front ? 3 : 2;
        }
    }
}