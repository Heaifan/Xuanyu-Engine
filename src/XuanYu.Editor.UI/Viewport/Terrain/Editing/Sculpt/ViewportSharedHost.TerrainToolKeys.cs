using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private bool TryTerrainToolKey(KeyEventArgs e)
    {
        bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        bool shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        if (ctrl && e.Key == Key.Z)
        {
            if (shift) _wb.RedoTerrain(); else _wb.UndoTerrain();
            return true;
        }
        if (ctrl && e.Key == Key.Y) { _wb.RedoTerrain(); return true; }
        if (e.KeyModifiers != KeyModifiers.None) return false;
        if (e.Key == Key.D1) { _wb.SelectTerrainElevationTool(false); return true; }
        if (e.Key == Key.D2) { _wb.SelectTerrainSmoothTool(); return true; }
        if (e.Key == Key.D3) { _wb.SelectTerrainFlattenTool(); return true; }
        if (e.Key == Key.D5) { _wb.SelectTerrainMountainTool(); return true; }
        if (e.Key == Key.D6) { _wb.SelectTerrainThermalTool(); return true; }
        if (e.Key == Key.D7) { _wb.SelectTerrainRainTool(); return true; }
        if (e.Key is not (Key.D4 or Key.E)) return false;
        _wb.SelectTerrainElevationTool(true);
        if (e.Key == Key.E)
        {
            var input = TopLevel.GetTopLevel(this)?.GetVisualDescendants().OfType<TextBox>()
                .FirstOrDefault(box => Equals(box.Tag, "TerrainTargetElevationInput"));
            input?.Focus();
            input?.SelectAll();
        }
        return true;
    }
}