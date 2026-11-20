using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private const int NativeLevel = 13;
    private const int NativeCacheLimit = 12;
    private static readonly bool G06DisableNative =
        Environment.GetEnvironmentVariable("XYE_G06_DISABLE_NATIVE") == "1";
    private readonly string _nativeSourceId;
    private readonly Dictionary<(GeoTileId, GeoTerrainEdgeNeighbors, GeoTerrainFootprintTile?),
        (RenderMesh Mesh, int EdgeMask)>
        _nativeMeshes = [];
    private readonly Queue<(GeoTileId, GeoTerrainEdgeNeighbors, GeoTerrainFootprintTile?)> _nativeOrder = new();
    private bool _firstNativeLogged;

    public bool NativeEnabled { get; private set; } = true;
    public int NativeDrawCount { get; private set; }

    public void ToggleNative() => NativeEnabled = !NativeEnabled;

    private void UpdateNativeDetails()
    {
        NativeDrawCount = 0;
        if (G06DisableNative || !NativeEnabled || Camera.DistanceMeters > 20_000d) return;
        GeoTileId[] visible = _visibleMeshes.Keys.ToArray();
        GeoTileId[] targets = visible.Where(id => id.Level == NativeLevel)
            .OrderBy(DistanceFromTarget).Take(NativeCacheLimit).ToArray();
        bool built = false;
        var ready = new Dictionary<GeoTileId, (RenderMesh Mesh, int EdgeMask)>();
        foreach (GeoTileId id in targets)
        {
            GeoTerrainFootprintTile? tile = _footprint is null ? null : _footprintTiles[id];
            GeoTerrainEdgeNeighbors neighbors = tile is null ? FindNeighbors(id, visible) : default;
            var key = (id, neighbors, tile);
            if (!_nativeMeshes.TryGetValue(key, out var entry))
            {
                if (built) continue;
                if (StreamingEnabled && !_source.PrepareNativeRegion(
                    GeographicTileScheme.Bounds(id))) continue;
                var mesh = tile is null ? BuildRectangularMesh(id, neighbors, true) : BuildFootprintMesh(tile, true);
                if (mesh is null) continue;
                entry = (mesh, 0);
                _nativeMeshes.Add(key, entry);
                _nativeOrder.Enqueue(key);
                while (_nativeMeshes.Count > NativeCacheLimit)
                    _nativeMeshes.Remove(_nativeOrder.Dequeue());
                built = true;
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
            var tile = _footprintTiles[id];
            var refined = GeoTerrainNativeEdges.Refine(tile, mask, _footprint!, _source, _anchor);
            var mesh = BuildFootprintMesh(refined, true);
            if (mesh is null) continue;
            _visibleMeshes[id] = mesh;
            var key = (id, default(GeoTerrainEdgeNeighbors), tile);
            if (_nativeMeshes.ContainsKey(key)) _nativeMeshes[key] = (mesh, mask);
        }
    }

    private double DistanceFromTarget(GeoTileId id)
    {
        GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
        double latitude = Math.Clamp(Camera.Target.LatitudeDegrees,
            bounds.SouthLatitudeDegrees, bounds.NorthLatitudeDegrees);
        double longitude = Math.Clamp(Camera.Target.LongitudeDegrees,
            bounds.WestLongitudeDegrees, bounds.EastLongitudeDegrees);
        double dy = Camera.Target.LatitudeDegrees - latitude;
        double dx = (Camera.Target.LongitudeDegrees - longitude) *
            Math.Cos(Camera.Target.LatitudeDegrees * Math.PI / 180d);
        return dx * dx + dy * dy;
    }
}
