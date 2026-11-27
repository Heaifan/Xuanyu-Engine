using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private long _nativeFirstBuilds, _nativeMaskRebuilds, _nativeTopologyMisses;
    private double _nativeFirstBuildMs, _nativeMaskRebuildMs;

    public long NativeFirstBuildCount => _nativeFirstBuilds;
    public long NativeMaskRebuildCount => _nativeMaskRebuilds;
    public long NativeTopologyMissCount => _nativeTopologyMisses;
    public long NativeActiveEvictionCount => _nativeMeshes.ActiveEvictionCount;

    private void RecordNativeBuild(bool mask, double elapsedMs)
    {
        if (mask)
        {
            _nativeMaskRebuilds++;
            _nativeMaskRebuildMs += elapsedMs;
        }
        else
        {
            _nativeFirstBuilds++;
            _nativeFirstBuildMs += elapsedMs;
        }
    }

    private void EndNativeProfile(int built, int topologyMisses, int requested)
    {
        _nativeTopologyMisses += topologyMisses;
        if (!_r02PerfEnabled || _monitorFrame % 120 != 0) return;
        Notice?.Invoke($"[G06/R08-04C] 帧={_monitorFrame} 活动={requested}/{NativeActiveBudget} " +
            $"缓存容量={NativeCacheCapacity} 首次预算={NativeFirstBuildBudget} 本帧首次建网={built} " +
            $"累计首次建网={_nativeFirstBuilds}/{_nativeFirstBuildMs:F1}ms " +
            $"累计共边整网重建={_nativeMaskRebuilds}/{_nativeMaskRebuildMs:F1}ms " +
            $"拓扑键未命中={_nativeTopologyMisses} 活动键淘汰={_nativeMeshes.ActiveEvictionCount} " +
            $"缓存淘汰={_nativeMeshes.EvictedCount} 拒绝写入={_nativeMeshes.RejectedInsertCount}");
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
