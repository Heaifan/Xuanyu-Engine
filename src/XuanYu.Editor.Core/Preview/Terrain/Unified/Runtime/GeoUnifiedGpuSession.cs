using System.Diagnostics;
using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Core.Preview.Terrain.Unified;

public sealed record GeoUnifiedGpuFrame(GeoUnifiedCoverage Coverage,
    GeoUnifiedCoverage? PendingCoverage, TerrainPoolDraw[] Draws,
    int SubmittedPages, bool CapacityBlocked)
{
    public double SelectionMs { get; init; }
}

public sealed partial class GeoUnifiedGpuSession
{
    private readonly GeoUnifiedLodSession _cpu;
    private readonly VkTerrainPagePool _pool;
    private readonly GeoCoordinate _origin;

    // A fresh pool belongs to one immutable CPU data generation and ENU origin.
    public GeoUnifiedGpuSession(GeoUnifiedLodSession cpu, VkTerrainPagePool pool, GeoCoordinate origin)
    {
        if (pool.ResidentCount != 0 || pool.PendingCount != 0)
            throw new ArgumentException("G07 会话需要独立的空页池。", nameof(pool));
        _cpu = cpu; _pool = pool; _origin = origin;
    }

    // Call after the previous graphics fence; the pool and CPU session have one serial owner.
    public GeoUnifiedGpuFrame Step(EnuPosition eye, int height, double fov,
        Matrix4x4 mvp, Func<GeoUnifiedTile, bool>? visible = null)
    {
        _pool.Poll();
        var coverage = _cpu.Coverage;
        GeoUnifiedCoverage? pending = null;
        double selectionMs = 0;
        if (coverage.Leaves.All(tile => _pool.IsReady(Key(tile))))
        {
            long start = Stopwatch.GetTimestamp();
            var selection = _cpu.Step(eye, height, fov, (tile, _) => _pool.IsReady(Key(tile)), visible);
            selectionMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            coverage = selection.Coverage;
            pending = selection.PendingCoverage;
        }
        var keep = coverage.Leaves.Concat(pending?.Leaves ?? []).Distinct().ToArray();
        bool blocked = keep.Length > _pool.Capacity;
        if (blocked) keep = coverage.Leaves.ToArray();
        _pool.Protect(keep.Select(Key));
        _pool.EvictUnprotected();
        int submitted = SubmitMissing(coverage, blocked ? null : pending);
        TerrainPoolDraw[] draws = coverage.Leaves.All(tile => _pool.IsReady(Key(tile)))
            ? coverage.Leaves.Select(tile => new TerrainPoolDraw(Key(tile), tile.South, tile.West,
                1 << tile.Level, _origin, mvp, coverage.EdgeMask(tile), _cpu.Domain?.ClipRects(tile))).ToArray() : [];
        return new(coverage, pending, draws, submitted, blocked) { SelectionMs = selectionMs };
    }

    // X and Y at level zero fit in 16 bits. Mask is topology, not height identity.
    private static long Key(GeoUnifiedTile tile) =>
        ((long)tile.Level << 32) | ((long)tile.X << 16) | (uint)tile.Y;
}