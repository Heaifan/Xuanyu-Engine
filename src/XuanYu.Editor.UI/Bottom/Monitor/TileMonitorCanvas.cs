using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI;

public sealed partial class TileMonitorCanvas : Control
{
    public GeoTileMonitorSnapshot? Snapshot { get; private set; }
    public event Action<GeoTileMonitorTile>? Selected;
    private GeoTileId? _selectedId;
    private Rect Plot => new(8, 6, Math.Max(1, Bounds.Width - 16),
        Math.Max(1, Bounds.Height - 12));

    public TileMonitorCanvas() => ClipToBounds = true;

    public void SetSnapshot(GeoTileMonitorSnapshot? snapshot, GeoTileId? selectedId)
    {
        Snapshot = snapshot;
        _selectedId = selectedId;
        InvalidateVisual();
    }

    private Point Project(double latitude, double longitude)
    {
        GeoTileBounds area = Snapshot!.Coverage;
        Rect plot = Plot;
        return new Point(plot.X + (longitude - area.WestLongitudeDegrees) /
            (area.EastLongitudeDegrees - area.WestLongitudeDegrees) * plot.Width,
            plot.Y + (area.NorthLatitudeDegrees - latitude) /
            (area.NorthLatitudeDegrees - area.SouthLatitudeDegrees) * plot.Height);
    }

    private Rect ProjectTile(GeoTileBounds tile)
    {
        GeoTileBounds area = Snapshot!.Coverage;
        Point nw = Project(Math.Min(tile.NorthLatitudeDegrees, area.NorthLatitudeDegrees),
            Math.Max(tile.WestLongitudeDegrees, area.WestLongitudeDegrees));
        Point se = Project(Math.Max(tile.SouthLatitudeDegrees, area.SouthLatitudeDegrees),
            Math.Min(tile.EastLongitudeDegrees, area.EastLongitudeDegrees));
        return new Rect(nw, se);
    }

    private bool InsideCoverage(GeoTileBounds tile)
    {
        GeoTileBounds area = Snapshot!.Coverage;
        return tile.EastLongitudeDegrees > area.WestLongitudeDegrees &&
            tile.WestLongitudeDegrees < area.EastLongitudeDegrees &&
            tile.NorthLatitudeDegrees > area.SouthLatitudeDegrees &&
            tile.SouthLatitudeDegrees < area.NorthLatitudeDegrees;
    }
}