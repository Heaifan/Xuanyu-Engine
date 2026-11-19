using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed record GeoTileMonitorFrame(
    GeoTileMonitorSnapshot Snapshot,
    GeoTerrainMeshCacheStats Mesh,
    NasademSourceResidency Source,
    int NativeDrawCount,
    int MiddleCount,
    int MiddleCapacity,
    long BackgroundLoads,
    DateTimeOffset CapturedAt);