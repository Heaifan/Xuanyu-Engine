using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private IReadOnlyList<GeoTileId> _monitorTargets = [];
    private long _monitorFrame;

    public GeoTileMonitorSnapshot ReadMonitor()
    {
        HashSet<GeoTileId> targets = _monitorTargets.ToHashSet();
        HashSet<GeoTileId> drawn = _visibleMeshes.Keys.ToHashSet();
        HashSet<GeoTileId> cached = _meshes.CachedKeys.ToHashSet();
        GeoTileId[] ids = targets.Union(drawn).Union(cached)
            .OrderBy(id => id.Level).ThenBy(id => id.Y).ThenBy(id => id.X)
            .ToArray();
        var tiles = new GeoTileMonitorTile[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            GeoTileId id = ids[i];
            GeoTileId? fallback = null;
            if (targets.Contains(id) && !drawn.Contains(id))
            {
                for (GeoTileId? parent = GeographicTileScheme.Parent(id);
                    parent is GeoTileId value; parent = GeographicTileScheme.Parent(value))
                {
                    if (!drawn.Contains(value)) continue;
                    fallback = value;
                    break;
                }
            }
            tiles[i] = DescribeMonitorTile(id, targets.Contains(id),
                drawn.Contains(id), cached.Contains(id), fallback);
        }
        return new GeoTileMonitorSnapshot(_monitorFrame, _coverage,
            Wgs84.ToGeo(Camera.PositionEcef), Camera.Target,
            Camera.DistanceMeters, Camera.YawRadians, SelectedCount, DrawCount,
            drawn.Count, PendingCount, StreamingEnabled, RawPrefetchPending, tiles);
    }
}
