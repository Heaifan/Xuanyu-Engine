using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using XuanYu.Editor.Pages;
namespace XuanYu.Editor.UI;
public sealed partial class ViewportSharedHost
{
    private void OnViewportPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this)) return;
        if (TryHandleBrushAdjustPress(e) || TryHandleSculptPress(e))
        {
            e.Handled = true;
            return;
        }
        var properties = e.GetCurrentPoint(this).Properties;
        bool middle = properties.IsMiddleButtonPressed;
        bool left = properties.IsLeftButtonPressed;
        if (middle || (_panMode && left))
        {
            _dragging = true;
            _panGesture = !middle || e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            _previous = e.GetPosition(this);
            e.Pointer.Capture(this);
            ToolTip.SetIsOpen(this, false);
            ClearGeoHudHover();
            Focus();
            e.Handled = true;
        }
        else if (left)
        {
            Focus();
            Point point = e.GetPosition(this);
            if (_wb.CurrentId == PageId.Terrain)
                ReportTerrainClick(point);
            else
            {
                double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
                Clicked?.Invoke((int)Math.Round(point.X * scale),
                    (int)Math.Round(point.Y * scale));
            }
            e.Handled = true;
        }
    }
    private void OnViewportPointerMoved(object? sender, PointerEventArgs e)
    {
        if (TryHandleBrushAdjustMove(e) || TryHandleSculptMove(e))
        {
            e.Handled = true;
            return;
        }
        Point next = e.GetPosition(this);
        if (!_dragging)
        {
            if (_wb.CurrentId == PageId.Terrain && _wb.HasTerrainHud)
            {
                ToolTip.SetIsOpen(this, false);
                if (ReferenceEquals(e.Source, this)) UpdateGeoHudHover(next);
                else ClearGeoHudHover();
            }
            else
                UpdateTerrainHover(next);
            return;
        }
        ToolTip.SetIsOpen(this, false);
        double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        OnCameraGesture(new ViewportCameraGesture(
            (float)((next.X - _previous.X) * scale),
            (float)((next.Y - _previous.Y) * scale), 0, _panGesture));
        _previous = next;
        e.Handled = true;
    }

    private void OnViewportPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (TryHandleSculptRelease(e))
        { UpdateGeoHudHover(e.GetPosition(this), force: true); e.Handled = true; return; }
        if (!_dragging) return;
        _dragging = false;
        e.Pointer.Capture(null);
        if (_wb.CurrentId == PageId.Terrain && _wb.HasTerrainHud)
            UpdateGeoHudHover(e.GetPosition(this), force: true);
        e.Handled = true;
    }

    private void OnViewportWheel(object? sender, PointerWheelEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this)) return;
        ToolTip.SetIsOpen(this, false);
        if (_sculptPointer is not null || _adjustOriginal is not null)
        { e.Handled = true; return; }
        Point cursor = e.GetPosition(this);
        double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        OnCameraGesture(new ViewportCameraGesture(0, 0,
            (float)e.Delta.Y, false,
            (float)(cursor.X * scale), (float)(cursor.Y * scale)));
        ClearGeoHudHover();
        e.Handled = true;
    }
}