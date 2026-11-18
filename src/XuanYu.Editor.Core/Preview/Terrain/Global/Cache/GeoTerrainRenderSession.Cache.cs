namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private const int InactiveMeshLimit = 64;
    private long _nextEvictionReport = 1;

    public GeoTerrainMeshCacheStats MeshCacheStats => new(
        _meshes.Count, _meshes.ActiveCount,
        _stitchedMeshes.Count, _stitchedMeshes.ActiveCount,
        _footprintMeshes.Count, _nativeMeshes.Count,
        _selectionMeshes.Count, _meshes.EvictedCount + _stitchedMeshes.EvictedCount);

    private void BeginMeshCacheFrame()
    {
        _meshes.BeginFrame();
        _stitchedMeshes.BeginFrame();
    }

    private void TrimMeshCaches()
    {
        _meshes.TrimInactive(InactiveMeshLimit);
        _stitchedMeshes.TrimInactive(InactiveMeshLimit);
        GeoTerrainMeshCacheStats s = MeshCacheStats;
        if (s.EvictedCount < _nextEvictionReport) return;
        _nextEvictionReport = s.EvictedCount + 128;
        Notice?.Invoke($"G-05D-01：基础网格 {s.BaseCount}（活跃 {s.BaseActiveCount}），" +
            $"共边 {s.StitchedCount}（活跃 {s.StitchedActiveCount}），" +
            $"Footprint {s.FootprintCount}，Native {s.NativeCount}，" +
            $"选择 {s.SelectionCount}，累计回收 {s.EvictedCount} 项");
    }
}