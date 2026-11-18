using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public readonly record struct TerrainStats(
    GeoTerrainMeshCacheStats Mesh,
    NasademSourceResidency Source,
    int DrawCount, int SelectedCount, int PendingCount,
    int MiddleCount, int MiddleCapacity, long BackgroundLoads);