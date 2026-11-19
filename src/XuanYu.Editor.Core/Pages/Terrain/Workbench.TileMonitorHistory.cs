using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public GeoTileMonitorSnapshot? TileMonitor => _geoPreview?.ReadMonitor();
    public GeoTileMonitorHistory? TileMonitorHistory => _geoPreview?.MonitorHistory;
}