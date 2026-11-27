using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private TerrainBrushSettings? _adjustOriginal;
    private Point _adjustStart;
    private bool _adjustStrength, _adjustFalloff;

    private bool TryHandleBrushKey(KeyEventArgs e)
    {
        if (_adjustOriginal is not null)
        {
            if (e.Key == Key.Escape) FinishBrushAdjust(false);
            else if (e.Key == Key.Enter) FinishBrushAdjust(true);
            return true;
        }
        if (!_wb.CanPreviewTerrainBrush) return false;
        if (_sculptPointer is not null) return true;
        if (TryTerrainToolKey(e)) return true;
        if (e.Key != Key.F) return false;
        if (_wb.IsTerrainMountainTool && e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
            !e.KeyModifiers.HasFlag(KeyModifiers.Alt)) return true;
        if (_sculptPointer is not null || _dragging || _panMode ||
            _brushPointer is not Point point) return true;
        _adjustOriginal = _wb.TerrainBrush;
        _adjustStart = point;
        _adjustFalloff = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _adjustStrength = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        ToolTip.SetIsOpen(this, false);
        return true;
    }

    private bool TryHandleBrushAdjustMove(PointerEventArgs e)
    {
        if (_adjustOriginal is not { } original) return false;
        double dx = e.GetPosition(this).X - _adjustStart.X;
        float radius = original.RadiusMeters;
        float strength = original.Strength;
        float falloff = original.Falloff;
        if (_adjustFalloff)
            falloff = (float)Math.Clamp(falloff + dx / 200d, 0d, 1d);
        else if (_adjustStrength)
            strength = (float)Math.Clamp(strength + dx / 200d, 0d, 1d);
        else
            radius = (float)Math.Clamp(radius * Math.Pow(2d, dx / 100d), 0.01d, 1000000d);
        _wb.PreviewTerrainBrush(new TerrainBrushSettings(radius, strength, falloff));
        return true;
    }

    private bool TryHandleBrushAdjustPress(PointerPressedEventArgs e)
    {
        if (_adjustOriginal is null) return false;
        var buttons = e.GetCurrentPoint(this).Properties;
        if (buttons.IsLeftButtonPressed) FinishBrushAdjust(true);
        else if (buttons.IsRightButtonPressed) FinishBrushAdjust(false);
        return true;
    }

    private void FinishBrushAdjust(bool commit)
    {
        if (_adjustOriginal is not { } original) return;
        _adjustOriginal = null;
        if (commit) _wb.CommitTerrainBrushPreview();
        else _wb.PreviewTerrainBrush(original);
    }
}