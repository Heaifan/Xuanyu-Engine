using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private readonly GeoTerrainMiniMap _miniMap = new();
    private readonly Grid _miniMapPanel = new()
    {
        Width = 180, Height = 180,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new Thickness(12), IsVisible = false
    };
    private long _lastMiniMapAt;

    private void UpdateGeoMiniMap()
    {
        bool visible = _wb.CurrentId == PageId.Terrain &&
            _wb.IsGeoTerrainPreview && _wb.GeoMiniMapVisible;
        _miniMapPanel.IsVisible = visible;
        if (!visible) return;
        ApplyMiniMapSize();
        long now = Environment.TickCount64;
        if (now - _lastMiniMapAt < 250) return;
        _lastMiniMapAt = now;
        var map = _wb.TerrainMiniMap();
        var camera = _wb.GeoTerrainCamera;
        if (map is null || camera is null)
        {
            _miniMapPanel.IsVisible = false;
            return;
        }
        _miniMap.Update(map, camera.Target.LatitudeDegrees,
            camera.Target.LongitudeDegrees, camera.YawRadians);
    }

    private void ApplyMiniMapSize()
    {
        int size = _wb.GeoMiniMapSize;
        if (_miniMapPanel.Width == size) return;
        _miniMapPanel.Width = _miniMapPanel.Height = size;
        _miniMap.Width = _miniMap.Height = size;
        _miniMap.InvalidateVisual();
    }
}