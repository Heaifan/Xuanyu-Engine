using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private Point _miniResizeStart;
    private int _miniResizeSize;
    private bool _miniResizing;

    private void AttachGeoOverlays()
    {
        _geoHudPanel.Children.Add(_geoHud);
        _miniMapPanel.Children.Add(_miniMap);
        AddMiniMapResizeEdge(true, false);
        AddMiniMapResizeEdge(false, true);
        AddMiniMapResizeEdge(true, true);
        Children.Add(_geoHudPanel);
        Children.Add(_miniMapPanel);
        ApplyMiniMapSize();
    }

    private void AddMiniMapResizeEdge(bool horizontal, bool vertical)
    {
        bool corner = horizontal && vertical;
        var edge = new Border
        {
            Width = horizontal ? (corner ? 14d : 10d) : double.NaN,
            Height = vertical ? (corner ? 14d : 10d) : double.NaN,
            HorizontalAlignment = horizontal
                ? HorizontalAlignment.Right : HorizontalAlignment.Stretch,
            VerticalAlignment = vertical
                ? VerticalAlignment.Bottom : VerticalAlignment.Stretch,
            Margin = corner ? new Thickness(0) : horizontal
                ? new Thickness(0, 0, 0, 14) : new Thickness(0, 0, 14, 0),
            Background = Brushes.Transparent,
            Cursor = new Cursor(corner ? StandardCursorType.BottomRightCorner
                : horizontal ? StandardCursorType.SizeWestEast
                : StandardCursorType.SizeNorthSouth)
        };
        edge.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(edge).Properties.IsLeftButtonPressed) return;
            _miniResizeStart = e.GetPosition(this);
            _miniResizeSize = _wb.GeoMiniMapSize;
            _miniResizing = true;
            e.Pointer.Capture(edge);
            e.Handled = true;
        };
        edge.PointerMoved += (_, e) =>
        {
            if (_miniResizing)
            {
                Point point = e.GetPosition(this);
                double dx = point.X - _miniResizeStart.X;
                double dy = point.Y - _miniResizeStart.Y;
                double delta = corner
                    ? Math.Abs(dx) >= Math.Abs(dy) ? dx : dy
                    : horizontal ? dx : dy;
                int size = (int)Math.Round(Math.Clamp(
                    _miniResizeSize + delta, 120d, 280d));
                _wb.SetGeoMiniMapSize(size, false);
                ApplyMiniMapSize();
            }
            e.Handled = true;
        };
        edge.PointerReleased += (_, e) =>
        {
            if (!_miniResizing) return;
            _miniResizing = false;
            e.Pointer.Capture(null);
            _wb.SaveGeoOverlayPreferences();
            e.Handled = true;
        };
        edge.PointerCaptureLost += (_, _) => _miniResizing = false;
        _miniMapPanel.Children.Add(edge);
    }
}