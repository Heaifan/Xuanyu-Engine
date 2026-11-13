
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private bool _terrainTipReady;

    private void UpdateTerrainHover(Point point)
    {
        Vector3? hit = PickTerrainAtPointer(point);
        if (hit is not { } position)
        {
            ToolTip.SetIsOpen(this, false);
            return;
        }

        if (!_terrainTipReady)
        {
            ToolTip.SetShowDelay(this, 0);
            ToolTip.SetShouldUseOverlayLayer(this, false);
            _terrainTipReady = true;
        }

        ToolTip.SetTip(this, FormatTerrainPoint(position));
        ToolTip.SetIsOpen(this, true);
    }

    private void ReportTerrainClick(Point point)
    {
        Vector3? hit = PickTerrainAtPointer(point);
        _wb.Log.Write(hit is { } position
            ? $"E4 地形定位：{FormatTerrainPoint(position)}"
            : "E4 地形定位：未命中地形");
    }

    private Vector3? PickTerrainAtPointer(Point point)
    {
        if (_wb.CurrentId != PageId.Terrain)
            return null;

        double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var (width, height) = PixelSize;
        return _wb.PickTerrainPoint(new Vector2(
            (float)(point.X * scale), (float)(point.Y * scale)),
            width, height);
    }

    private static string FormatTerrainPoint(Vector3 point) =>
        $"X {point.X:0.00} · Y {point.Y:0.00} · Z {point.Z:0.00}";
}
