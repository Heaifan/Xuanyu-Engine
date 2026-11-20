using System.Runtime.CompilerServices;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly NasademHgtInfo[] _collisionTiles;
    private readonly ConditionalWeakTable<RenderMesh, GeoContourSurface> _collisionSurfaces = new();

    private double SweepZoom(EcefPosition start, EcefPosition end, double clearance)
    {
        double dx = end.X - start.X, dy = end.Y - start.Y, dz = end.Z - start.Z;
        double length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (length == 0d) return 1d;
        EcefPosition At(double t) => new(start.X + t * dx, start.Y + t * dy, start.Z + t * dz);
        GeoCoordinate first = Wgs84.ToGeo(start), last = Wgs84.ToGeo(end);
        double HeightSlope(GeoCoordinate point)
        {
            double lat = point.LatitudeDegrees * Math.PI / 180d, lon = point.LongitudeDegrees * Math.PI / 180d;
            return (dx * Math.Cos(lon) + dy * Math.Sin(lon)) * Math.Cos(lat) + dz * Math.Sin(lat);
        }
        bool monotone = HeightSlope(first) >= 0d || HeightSlope(last) <= 0d;
        double limit = Trace(0d, 1d, first, last);
        if (_visibleMeshes.Count == 0 || limit == 0d) return limit;
        // Rendered triangles and the eye share a linear ENU space.
        EnuPosition a = _anchor.ToEnu(start), b = _anchor.ToEnu(At(limit));
        double fraction = 1d;
        var meshFootprint = ZoomFootprint.Around(Wgs84.ToGeo(At(limit * 0.5d)), length * limit * 0.5d);
        foreach (var (id, mesh) in _visibleMeshes)
        {
            GeoTileBounds tile = GeographicTileScheme.Bounds(id);
            if (!meshFootprint.Overlaps(tile)) continue;
            var surface = _collisionSurfaces.GetValue(mesh, static value => new(value));
            fraction = Math.Min(fraction, surface.Sweep(a, b, clearance));
        }
        return fraction < 1d ? Math.Max(0d, limit * fraction - 0.001d / length) : limit;

        double Trace(double lo, double hi, GeoCoordinate left, GeoCoordinate right)
        {
            // Geodetic height is convex along an exterior chord. For a cursor
            // translation with an interior minimum, use a supporting plane.
            double lower = Math.Min(left.HeightMeters, right.HeightMeters) - clearance;
            if (monotone && lower >= short.MaxValue) return hi;
            double mid = (lo + hi) * 0.5d, radius = (hi - lo) * length * 0.5d;
            if (!monotone && radius > 6_000_000d) return lo;
            GeoCoordinate center = Wgs84.ToGeo(At(mid));
            if (!monotone)
                lower = center.HeightMeters - Math.Abs(HeightSlope(center)) * (hi - lo) * 0.5d - clearance;
            if (lower >= short.MaxValue) return hi;
            var disk = ZoomFootprint.Around(center, radius);
            if (lower >= BoundHeight(disk, radius)) return hi;
            if (radius <= 0.0005d) return lo;
            double safe = Trace(lo, mid, left, center);
            return safe < mid ? safe : Trace(mid, hi, center, right);
        }
    }

    private double BoundHeight(ZoomFootprint disk, double radius)
    {
        double highest = double.NegativeInfinity;
        bool covered = false;
        foreach (var tile in _collisionTiles)
        {
            double south = tile.SouthLatitude, west = tile.WestLongitude;
            var bounds = new GeoTileBounds(south, west, south + 1d, west + 1d);
            if (!disk.Overlaps(bounds)) continue;
            if (radius > 4096d) return short.MaxValue;
            double lon = disk.LongitudeNear(west + 0.5d);
            double x0 = lon - disk.LongitudeRadius, x1 = lon + disk.LongitudeRadius;
            if (disk.LongitudeRadius > 179.5d) (x0, x1) = (west, west + 1d);
            highest = Math.Max(highest, _source.BoundRawHeight(tile, x0, x1, disk.South, disk.North));
            covered |= x0 >= west && x1 <= west + 1d && disk.South >= south && disk.North <= south + 1d;
        }
        return covered ? highest : Math.Max(0d, highest);
    }
}
