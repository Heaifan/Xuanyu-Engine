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

    // 命中点及三角形坐标与射线使用同一相对原点；默认是数据集 ENU。
    public bool TryPickSurfaceHit(Vector3 origin, Vector3 ray, out GeoUnifiedSurfaceHit hit,
        EnuPosition renderOrigin = default)
    {
        float closest = float.PositiveInfinity;
        hit = default;
        foreach (var entry in SurfaceEntries())
        {
            if (!GeoUnifiedSurface.Intersects(entry.Bounds.RelativeTo(renderOrigin), origin, ray, closest)) continue;
            using var lease = _cache.TryAcquire(entry.Tile);
            if (lease is null) continue;
            GeoUnifiedSurface.Pick(lease.Page, _anchor, entry.Mask, entry.Clips, origin, ray, renderOrigin, ref closest, ref hit);
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

    public GeoElevationSample SampleElevation(GeoCoordinate position)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _source.SampleElevation(position);
    }

    public double? SampleImportedHeight(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || !double.IsFinite(longitude) ||
            latitude is < -90d or > 90d || longitude is < -180d or > 180d)
            return null;
        return SampleElevation(new GeoCoordinate(latitude, longitude, 0d)).HeightMeters;
    }
}