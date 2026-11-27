using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Core.Preview.Terrain.Unified;

public sealed partial class GeoUnifiedGpuSession
{
    private int SubmitMissing(GeoUnifiedCoverage current, GeoUnifiedCoverage? pending)
    {
        int limit = Math.Min(_pool.AvailableSlots, _pool.UploadLimit);
        if (_pool.PendingCount != 0 || limit == 0) return 0;
        var requests = new List<TerrainPoolUpload>(limit);
        Collect(current, requests, limit);
        if (pending is not null && requests.Count < limit) Collect(pending, requests, limit);
        return _pool.Submit(requests);
    }

    private void Collect(GeoUnifiedCoverage coverage, List<TerrainPoolUpload> requests, int limit)
    {
        foreach (var tile in coverage.Leaves)
        {
            if (requests.Count == limit) return;
            long key = Key(tile);
            if (_pool.Contains(key) || requests.Any(item => item.Key == key)) continue;
            using var lease = _cpu.AcquirePage(tile, coverage.EdgeMask(tile));
            if (lease is null) continue;
            var heights = new short[33 * 33];
            for (int y = 0; y < 33; y++)
                for (int x = 0; x < 33; x++) heights[y * 33 + x] = lease.Page.HeightAt(x, y)!.Value;
            requests.Add(new(key, heights));
        }
    }
}