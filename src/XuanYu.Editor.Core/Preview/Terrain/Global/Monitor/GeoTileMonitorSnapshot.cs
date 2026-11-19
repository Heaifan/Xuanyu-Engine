using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed record GeoTileMonitorSnapshot(
    long Frame, GeoTileBounds Coverage, GeoCoordinate Eye, GeoCoordinate Target,
    double CameraDistanceMeters, double CameraYawRadians,
    int TargetCount, int DrawCount, int VisibleCount, int PendingCount,
    bool StreamingEnabled, bool RawPrefetchPending,
    IReadOnlyList<GeoTileMonitorTile> Tiles);