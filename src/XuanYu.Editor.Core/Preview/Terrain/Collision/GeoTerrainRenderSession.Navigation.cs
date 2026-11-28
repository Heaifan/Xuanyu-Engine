using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public void SnapAxis(Vector3 axis, uint width, uint height)
    {
        if (axis.Z < -0.5f) return;
        if (axis.Z <= 0.5f)
        {
            Camera.SnapAxis(axis);
            return;
        }
        double spanE = 0d, spanN = 0d;
        foreach (double lat in new[] { _coverage.SouthLatitudeDegrees,
            _coverage.NorthLatitudeDegrees })
            foreach (double lon in new[] { _coverage.WestLongitudeDegrees,
                _coverage.EastLongitudeDegrees })
            {
                EnuPosition pos = _anchor.ToEnu(Wgs84.ToEcef(
                    new GeoCoordinate(lat, lon, 0d)));
                spanE = Math.Max(spanE, Math.Abs(pos.East));
                spanN = Math.Max(spanN, Math.Abs(pos.North));
            }
        double aspect = Math.Max(1u, width) / (double)Math.Max(1u, height);
        double tangent = Math.Tan(Math.PI / 6d);
        double fit = Math.Max(spanN / tangent, spanE / (tangent * aspect));
        Camera.FrameTop(_anchor.Origin, fit * 1.20d + 6_000d);
        Notice?.Invoke("G-03F：Z 正顶视已摆正，并聚焦完整 HGT 范围");
    }

    private bool TryPickZoomAnchor(Vector2 pixel, uint width, uint height,
        out Vector3 anchor)
    {
        anchor = default;
        Matrix4x4 projection = ViewProjection(width, height);
        if (_g07Geoid is null && GeoTerrainCursorPicker.TryPick(projection, pixel, width,
            height, _visibleMeshes.Values, out anchor)) return true;
        if (!ViewportScreenRay.TryCreate(projection, pixel, width, height,
            out Vector3 start, out Vector3 ray) || MathF.Abs(ray.Z) < 1e-4f)
            return false;
        double z = 0d;
        for (int i = 0; i < 4; i++)
        {
            double distance = (z - start.Z) / ray.Z;
            if (!double.IsFinite(distance) || distance <= 0d) return false;
            double east = start.X + distance * ray.X;
            double north = start.Y + distance * ray.Y;
            GeoCoordinate geo = Wgs84.ToGeo(_anchor.ToEcef(
                new EnuPosition(east, north, z)));
            double? sampled = _source.Sample(geo.LatitudeDegrees,
                geo.LongitudeDegrees);
            if (sampled is null) return false;
            z = _anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(
                geo.LatitudeDegrees, geo.LongitudeDegrees, G07EllipsoidHeight(geo, sampled.Value)))).Up;
        }
        double t = (z - start.Z) / ray.Z;
        anchor = new Vector3((float)(start.X + ray.X * t),
            (float)(start.Y + ray.Y * t), (float)z);
        return t > 0d;
    }
}