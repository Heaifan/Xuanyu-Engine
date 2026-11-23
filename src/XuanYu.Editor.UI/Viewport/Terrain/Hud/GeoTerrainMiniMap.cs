using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

internal sealed partial class GeoTerrainMiniMap : Control
{
    private GeoTerrainMiniMapData? _map;
    private double _latitude, _longitude, _yaw;

    public GeoTerrainMiniMap()
    {
        Width = 180;
        Height = 180;
        ClipToBounds = true;
        IsHitTestVisible = false;
    }

    public void Update(GeoTerrainMiniMapData map,
        double latitude, double longitude, double yaw)
    {
        if (ReferenceEquals(_map, map) && _latitude == latitude &&
            _longitude == longitude && _yaw == yaw) return;
        _map = map;
        _latitude = latitude;
        _longitude = longitude;
        _yaw = yaw;
        InvalidateVisual();
    }
}