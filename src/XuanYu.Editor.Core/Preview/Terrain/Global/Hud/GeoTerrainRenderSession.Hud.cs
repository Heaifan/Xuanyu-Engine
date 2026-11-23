using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public GeoTerrainHudPoint? PickHudPoint(Vector2 pixel, uint width, uint height)
    {
        if (width < 2 || height < 2 || _visibleMeshes.Count == 0) return null;
        if (!GeoTerrainCursorPicker.TryPick(ViewProjection(width, height), pixel,
            width, height, _visibleMeshes.Values, out Vector3 hit)) return null;
        GeoCoordinate geo = Wgs84.ToGeo(_anchor.ToEcef(
            new EnuPosition(hit.X, hit.Y, hit.Z)));
        int south = (int)Math.Floor(geo.LatitudeDegrees);
        int west = (int)Math.Floor(geo.LongitudeDegrees);
        string tile = $"{(south < 0 ? "S" : "N")}{Math.Abs(south):00}" +
            $"{(west < 0 ? "W" : "E")}{Math.Abs(west):000}";
        return new GeoTerrainHudPoint(geo.LatitudeDegrees,
            geo.LongitudeDegrees, geo.HeightMeters, tile);
    }

    public double? HudMetersPerPixel(uint width, uint height)
    {
        if (width < 4 || height < 4) return null;
        Matrix4x4 projection = ViewProjection(width, height);
        float centerX = width * 0.5f, centerY = height * 0.5f;
        float half = Math.Min(60f, width * 0.25f);
        float z = (float)_anchor.ToEnu(Camera.TargetEcef).Up;
        if (!TryPlanePoint(projection, new(centerX - half, centerY),
                width, height, z, out Vector3 a) ||
            !TryPlanePoint(projection, new(centerX + half, centerY),
                width, height, z, out Vector3 b)) return null;
        double meters = Vector2.Distance(new Vector2(a.X, a.Y),
            new Vector2(b.X, b.Y));
        return double.IsFinite(meters) && meters > 0d
            ? meters / (2d * half) : null;
    }

    private static bool TryPlanePoint(Matrix4x4 projection, Vector2 pixel,
        uint width, uint height, float z, out Vector3 point)
    {
        point = default;
        if (!ViewportScreenRay.TryCreate(projection, pixel, width, height,
            out Vector3 start, out Vector3 direction) ||
            MathF.Abs(direction.Z) < 0.03f) return false;
        float t = (z - start.Z) / direction.Z;
        if (!float.IsFinite(t) || t <= 0f) return false;
        point = start + direction * t;
        return float.IsFinite(point.X) && float.IsFinite(point.Y);
    }
}