namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private const int InactiveMeshLimit = 64;

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
    }
}