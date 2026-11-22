using System.Diagnostics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private HashSet<(int South, int West)>? _demImported;

    private RenderDemTile? ReadResidentDem(
        Dictionary<(int South, int West), RenderDemTile?> resident, int south, int west)
    {
        var key = (south, west);
        if (resident.TryGetValue(key, out var cached)) return cached;
        NasademTile? tile = _source.TryGetResidentTile(south, west);
        RenderDemTile? dem = tile is null ? null : new RenderDemTile(
            tile.SouthLatitude, tile.WestLongitude, tile.SamplesNorthToSouth);
        resident.Add(key, dem);
        return dem;
    }

    private void UpdateVisibleDemResidency(
        Dictionary<(int South, int West), RenderDemTile?> resident,
        int crossing, int built, int bound, int drawCount, long start)
    {
        _demImported ??= _collisionTiles.Select(t => (t.SouthLatitude, t.WestLongitude)).ToHashSet();
        var missing = new List<(int South, int West)>();
        int imported = 0;
        var outside = new List<(int South, int West)>();
        foreach (var (key, dem) in resident)
        {
            if (!_demImported.Contains(key)) { outside.Add(key); continue; }
            imported++;
            if (!dem.HasValue) missing.Add(key);
        }
        bool budgetLimited = imported > SourceResidency.Capacity;
        if (StreamingEnabled && !budgetLimited && !_source.RawPrefetchPending && missing.Count > 0)
        {
            var next = missing.OrderBy(DemDistanceFromTarget).First();
            _source.PrefetchRaw(next.South, next.West);
        }
        if (_r02PerfEnabled && (built > 0 || _monitorFrame % 120 == 0))
            Notice?.Invoke($"[G06/R05B] 帧={_monitorFrame} 跨界输入={crossing} 新切分={built} " +
                $"已导入需求={imported} 已驻留={imported - missing.Count} 未驻留={missing.Count} " +
                $"未导入={outside.Count} DEM附加={bound}/{drawCount} 配额受限={budgetLimited} " +
                $"在途={_source.RawPrefetchPending} DEM绑定={Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1}ms " +
                $"未驻留坐标=[{string.Join(",", missing)}] 未导入坐标=[{string.Join(",", outside)}]");
    }

    private double DemDistanceFromTarget((int South, int West) key)
    {
        var target = Camera.Target;
        double dy = target.LatitudeDegrees - Math.Clamp(target.LatitudeDegrees, key.South, key.South + 1d);
        double dx = target.LongitudeDegrees - Math.Clamp(target.LongitudeDegrees, key.West, key.West + 1d);
        dx *= Math.Cos(target.LatitudeDegrees * Math.PI / 180d);
        return dx * dx + dy * dy;
    }
}