namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoTerrainMeshCacheStats(
    int BaseCount, int BaseActiveCount, int StitchedCount, int StitchedActiveCount,
    int FootprintCount, int NativeCount, int SelectionCount, long EvictedCount)
{
    public int BaseInactiveCount => BaseCount - BaseActiveCount;
    public int StitchedInactiveCount => StitchedCount - StitchedActiveCount;
}