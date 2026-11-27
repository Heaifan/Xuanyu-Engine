using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;
public sealed partial class GeoTerrainRenderSession
{
    private const int NativeLevel = 13;
    private static readonly int NativeActiveBudget = Environment.GetEnvironmentVariable("XYE_G06_NATIVE_LIMIT") switch { "12" => 12, "128" => 128, _ => 24 };
    private const int NativeCacheCapacity = 128;
    private const int NativeFirstBuildBudget = 1;
    private static readonly bool G06DisableNative =
        Environment.GetEnvironmentVariable("XYE_G06_DISABLE_NATIVE") == "1";
    private readonly string _nativeSourceId;
    private readonly GeoTerrainNativeMeshCache<
        (GeoTileId Id, GeoTerrainEdgeNeighbors Neighbors, GeoTerrainFootprintTile? Topology),
        (RenderMesh Mesh, int EdgeMask)> _nativeMeshes = new(NativeCacheCapacity);
    private bool _firstNativeLogged;
    public bool NativeEnabled { get; private set; } = true;
    public int NativeDrawCount { get; private set; }
    public int NativeRequestedCount { get; private set; }
    public int NativeCacheCapacityLimit => NativeCacheCapacity;
    public void ToggleNative() => NativeEnabled = !NativeEnabled;

    private void UpdateNativeDetails()
    {
        NativeDrawCount = 0;
        if (G06DisableNative || !NativeEnabled || Camera.DistanceMeters > 20_000d)
        {
            EndNativeProfile(0, 0, 0);
            NativeRequestedCount = 0;
            return;
        }
        GeoTileId[] visible = _visibleMeshes.Keys.ToArray();
        GeoTileId[] eligible = visible.Where(id => id.Level == NativeLevel)
            .OrderBy(DistanceFromTarget).ToArray();
        GeoTileId[] targets = eligible.Take(NativeActiveBudget).ToArray();
        NativeRequestedCount = targets.Length;
        _nativeMeshes.BeginFrame(targets.Select(id => (id,
            _footprint is null ? FindNeighbors(id, visible) : default,
            _footprint is null ? null : _footprintTiles[id])));
        int waitingBudget = 0, waitingSource = 0, missingMesh = 0;
        int built = 0, topologyMisses = 0;
        var ready = new Dictionary<GeoTileId, (RenderMesh Mesh, int EdgeMask)>();
        foreach (GeoTileId id in targets)
        {
            GeoTerrainFootprintTile? tile = _footprint is null ? null : _footprintTiles[id];
            var key = (id, tile is null ? FindNeighbors(id, visible) : default, tile);
            tile = key.Item3;
            GeoTerrainEdgeNeighbors neighbors = key.Item2;
            if (!_nativeMeshes.TryGet(key, out var entry))
            {
                if (built >= NativeFirstBuildBudget) { waitingBudget++; continue; }
                bool topologyMiss = _nativeMeshes.ContainsMatching(old => old.Id == id &&
                    (tile is null ? old.Neighbors != neighbors : !ReferenceEquals(old.Topology, tile)));
                if (StreamingEnabled && !_source.PrepareNativeRegion(
                    GeographicTileScheme.Bounds(id))) { waitingSource++; continue; }
                long buildStarted = System.Diagnostics.Stopwatch.GetTimestamp();
                var mesh = tile is null ? BuildRectangularMesh(id, neighbors, true) : BuildFootprintMesh(tile, true);
                if (mesh is null) { missingMesh++; continue; }
                if (topologyMiss) topologyMisses++;
                RecordNativeBuild(false, System.Diagnostics.Stopwatch.GetElapsedTime(buildStarted).TotalMilliseconds);
                entry = (mesh, 0);
                _nativeMeshes.Set(key, entry);
                built++;
                if (!_firstNativeLogged)
                {
                    _firstNativeLogged = true;
                    Notice?.Invoke("G-04-02：近景原始采样网格已开始接管可见瓦片");
                }
            }
            _visibleMeshes[id] = entry.Mesh;
            if (tile is not null) ready.Add(id, entry);
            NativeDrawCount++;
        }
        foreach (var (id, entry) in ready)
        {
            int mask = GeoTerrainNativeEdges.Mask(id, ready.ContainsKey);
            if (mask == entry.EdgeMask) continue;
            long rebuildStarted = System.Diagnostics.Stopwatch.GetTimestamp();
            var tile = _footprintTiles[id];
            var refined = GeoTerrainNativeEdges.Refine(tile, mask, _footprint!, _source, _anchor);
            var mesh = BuildFootprintMesh(refined, true);
            if (mesh is null) continue;
            RecordNativeBuild(true, System.Diagnostics.Stopwatch.GetElapsedTime(rebuildStarted).TotalMilliseconds);
            _visibleMeshes[id] = mesh;
            var key = (id, default(GeoTerrainEdgeNeighbors), tile);
            _nativeMeshes.Set(key, (mesh, mask));
        }
        if (_g06DiagnosticEnabled && _monitorFrame % 120 == 0)
            Notice?.Invoke($"[G06/R08] 帧={_monitorFrame} 距离={Camera.DistanceMeters:F0}m " +
                $"覆盖={visible.Length} L13={eligible.Length} 非L13={visible.Length - eligible.Length} " +
                $"活动配额={NativeActiveBudget} 缓存容量={NativeCacheCapacity} 首次建网预算={NativeFirstBuildBudget} " +
                $"超额={eligible.Length - targets.Length} Native={NativeDrawCount}/{targets.Length} " +
                $"待预算={waitingBudget} 待数据={waitingSource} 无网格={missingMesh} " +
                $"缓存={_nativeMeshes.Count} 地表三角形={_visibleMeshes.Values.Sum(m => (long)m.TriangleCount)}");
        EndNativeProfile(built, topologyMisses, targets.Length);
    }
}
