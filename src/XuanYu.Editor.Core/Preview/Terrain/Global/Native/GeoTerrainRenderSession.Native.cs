using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private const int NativeLevel = 13;
    private const int NativeCacheLimit = 12;
    private readonly NasademTile _nativeTile;
    private readonly Dictionary<(GeoTileId, GeoTerrainEdgeNeighbors), RenderMesh>
        _nativeMeshes = [];
    private readonly Queue<(GeoTileId, GeoTerrainEdgeNeighbors)> _nativeOrder = new();
    private bool _firstNativeLogged;

    public bool NativeEnabled { get; private set; } = true;
    public int NativeDrawCount { get; private set; }

    public void ToggleNative() => NativeEnabled = !NativeEnabled;

    private void UpdateNativeDetails()
    {
        NativeDrawCount = 0;
        if (!NativeEnabled || Camera.DistanceMeters > 20_000d) return;
        GeoTileId[] visible = _visibleMeshes.Keys.ToArray();
        GeoTileId[] targets = visible.Where(id => id.Level == NativeLevel)
            .OrderBy(DistanceFromTarget).Take(NativeCacheLimit).ToArray();
        bool built = false;
        foreach (GeoTileId id in targets)
        {
            GeoTerrainEdgeNeighbors neighbors = FindNeighbors(id, visible);
            var key = (id, neighbors);
            if (!_nativeMeshes.TryGetValue(key, out RenderMesh? mesh))
            {
                if (built) continue;
                mesh = GeoTerrainNativeMeshBuilder.Build(_nativeTile, id,
                    _anchor, _source, _coverage, neighbors);
                if (mesh is null) continue;
                _nativeMeshes.Add(key, mesh);
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
            _visibleMeshes[id] = mesh;
            NativeDrawCount++;
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