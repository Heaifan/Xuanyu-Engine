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

    private void UpdateGeoHudHover(Point point, bool force = false)
    {
        long now = Environment.TickCount64;
        if (!_wb.GeoHudVisible || !force && now - _lastHudPickAt < 120) return;
        _lastHudPickAt = now;
        double scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;
        var (width, height) = PixelSize;
        _geoHud.ShowPoint(_wb.PickTerrainHud(new Vector2(
            (float)(point.X * scaling), (float)(point.Y * scaling)), width, height));
    }

    private void UpdateGeoHudScale()
    {
        bool visible = _wb.CurrentId == PageId.Terrain &&
            _wb.IsGeoTerrainPreview && _wb.GeoHudVisible;
        bool wasVisible = _geoHudPanel.IsVisible;
        _geoHudPanel.IsVisible = visible;
        if (!visible)
        {
            if (wasVisible) ClearGeoHudHover();
            return;
        }
        var (width, height) = PixelSize;
        double scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;
        _geoHud.ShowScale(GeoTerrainHudScale.From(
            _wb.TerrainHudMetersPerPixel(width, height), scaling));
    }

    private void ClearGeoHudHover() => _geoHud.ShowPoint(null);
}