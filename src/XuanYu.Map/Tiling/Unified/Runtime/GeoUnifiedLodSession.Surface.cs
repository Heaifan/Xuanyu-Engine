using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    private sealed record SurfaceEntry(GeoUnifiedTile Tile, int Mask, GeoUnifiedBounds Bounds, Vector4[]? Clips);
    private GeoUnifiedCoverage? _surfaceCoverage;
    private SurfaceEntry[] _surfaceEntries = [];

    public bool TryPickSurface(Vector3 origin, Vector3 ray, out EnuPosition hit)
    {
        bool found = TryPickSurfaceHit(origin, ray, out var surface);
        hit = new(surface.Position.X, surface.Position.Y, surface.Position.Z);
        return found;
    }

    public bool TryPickSurfaceHit(Vector3 origin, Vector3 ray, out GeoUnifiedSurfaceHit hit)
    {
        float closest = float.PositiveInfinity;
        hit = default;
        foreach (var entry in SurfaceEntries())
        {
            if (!GeoUnifiedSurface.Intersects(entry.Bounds, origin, ray, closest)) continue;
            using var lease = _cache.TryAcquire(entry.Tile);
            if (lease is null) continue;
            GeoUnifiedSurface.Pick(lease.Page, _anchor, entry.Mask, entry.Clips, origin, ray, ref closest, ref hit);
        }
        return float.IsFinite(closest);
    }

    private SurfaceEntry[] SurfaceEntries()
    {
        if (ReferenceEquals(_surfaceCoverage, Coverage)) return _surfaceEntries;
        var entries = new List<SurfaceEntry>(Coverage.Leaves.Count);
        bool complete = true;
        foreach (var tile in Coverage.Leaves)
        {
            var clips = Domain?.ClipRects(tile);
            if (clips is { Length: 0 }) continue;
            int mask = Coverage.EdgeMask(tile);
            if (_resources.TryGetValue((tile, mask), out var task) && task.IsCompletedSuccessfully)
                entries.Add(new(tile, mask, task.Result.Bounds, clips));
            else complete = false;
        }
        _surfaceEntries = entries.ToArray();
        _surfaceCoverage = complete ? Coverage : null;
        return _surfaceEntries;
    }

    public double? SampleGridHeight(double latitude, double longitude) =>
        Domain is { } domain && !domain.Contains(latitude, longitude)
            ? 0d : SampleImportedHeight(latitude, longitude);

    public double? SampleImportedHeight(double latitude, double longitude)
    {
        double x = (longitude + 180d) * 3600d, y = (latitude + 90d) * 3600d;
        int left = (int)Math.Floor(x), right = (int)Math.Ceiling(x);
        int bottom = (int)Math.Floor(y), top = (int)Math.Ceiling(y);
        var a = _source.SampleRaw(left, bottom).Height;
        var b = _source.SampleRaw(right, bottom).Height;
        var c = _source.SampleRaw(left, top).Height;
        var d = _source.SampleRaw(right, top).Height;
        if (a is null || b is null || c is null || d is null) return null;
        double u = x - left, v = y - bottom;
        return (a.Value * (1 - u) + b.Value * u) * (1 - v) +
            (c.Value * (1 - u) + d.Value * u) * v;
    }
}