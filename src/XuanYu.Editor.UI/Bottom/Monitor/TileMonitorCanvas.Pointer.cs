using Avalonia.Input;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

public sealed partial class TileMonitorCanvas
{
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (Snapshot is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        var point = e.GetPosition(this);
        GeoTileMonitorTile? tile = Snapshot.Tiles
            .Where(t => InsideCoverage(t.Bounds) && ProjectTile(t.Bounds).Contains(point))
            .OrderByDescending(t => t.Targeted ? 2 : t.Drawn ? 1 : 0)
            .ThenByDescending(t => t.Id.Level)
            .Select(t => (GeoTileMonitorTile?)t)
            .FirstOrDefault();
        if (tile is null) return;
        _selectedId = tile.Value.Id;
        InvalidateVisual();
        Selected?.Invoke(tile.Value);
        e.Handled = true;
    }
}