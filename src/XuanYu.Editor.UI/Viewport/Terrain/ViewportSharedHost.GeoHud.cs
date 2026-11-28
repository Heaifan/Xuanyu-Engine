using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private readonly GeoTerrainHud _geoHud = new();
    private readonly Grid _geoHudPanel = new()
    {
        Width = 210,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new Thickness(12), IsVisible = false
    };
    private long _lastHudPickAt;
    private bool? _hudGeoMode;
    private Point? _pendingG07HudPoint;

    private void UpdateGeoHudHover(Point point, bool force = false)
    {
        long now = Environment.TickCount64;
        if (_wb.CurrentId != PageId.Terrain || !_wb.HasTerrainHud ||
            !_wb.GeoHudVisible || !force && now - _lastHudPickAt < 120) return;
        _lastHudPickAt = now;
        if (_wb.G07PreviewSource is not null)
        {
            _pendingG07HudPoint = point;
            return;
        }
        double scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;
        var (width, height) = PixelSize;
        var pixel = new Vector2((float)(point.X * scaling), (float)(point.Y * scaling));
        if (_wb.IsGeoTerrainPreview) _geoHud.ShowPoint(_wb.PickTerrainHud(pixel, width, height));
        else _geoHud.ShowLocalPoint(_wb.PickLocalTerrainHud(pixel, width, height));
    }

    private void UpdateGeoHudScale()
    {
        bool visible = _wb.CurrentId == PageId.Terrain &&
            _wb.HasTerrainHud && _wb.GeoHudVisible;
        bool wasVisible = _geoHudPanel.IsVisible;
        _geoHudPanel.IsVisible = visible;
        if (!visible)
        {
            if (wasVisible) ClearGeoHudHover();
            return;
        }
        if (_hudGeoMode != _wb.IsGeoTerrainPreview)
        {
            _hudGeoMode = _wb.IsGeoTerrainPreview;
            ClearGeoHudHover();
        }
        var (width, height) = PixelSize;
        double scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;
        _geoHud.ShowScale(GeoTerrainHudScale.From(
            _wb.TerrainHudMetersPerPixel(width, height), scaling));
    }

    private void ClearGeoHudHover()
    {
        _pendingG07HudPoint = null;
        if (_wb.IsGeoTerrainPreview) _geoHud.ShowPoint(null);
        else _geoHud.ShowLocalPoint(null);
    }
}