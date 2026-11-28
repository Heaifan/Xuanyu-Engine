using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public void ZoomAtCursor(float wheel, Vector2 pixel, uint width, uint height)
    {
        if (width < 2 || height < 2) return;
        // 侧视时射线与水平锚点面近乎平行，不做不稳定的锚点反解。
        Vector3 anchor = default;
        bool found = Camera.PitchRadians > 0.02d &&
            TryPickZoomAnchor(pixel, width, height, out anchor);
        double before = Camera.DistanceMeters;
        double requested = Math.Clamp(before * Math.Pow(0.85d, wheel),
            GlobalCamera.MinDistanceMeters, GlobalCamera.MaxDistanceMeters);
        Camera.Zoom(wheel);
        if (!found || Camera.DistanceMeters == before) return;
        // 原始地形已截停向前缩放时，不再平移眼点绕过这次安全边界。
        if (wheel > 0f && Camera.DistanceMeters > requested) return;

        for (int i = 0; i < 2; i++)
        {
            if (!ViewportScreenRay.TryCreate(ViewProjection(width, height),
                pixel, width, height, out Vector3 start, out Vector3 ray) ||
                MathF.Abs(ray.Z) < 1e-4f) return;
            float distance = (anchor.Z - start.Z) / ray.Z;
            if (!float.IsFinite(distance) || distance <= 0f) return;
            Vector3 atHeight = start + ray * distance;
            GeoCoordinate target = CursorTarget(anchor, atHeight);
            if (_source.Sample(target.LatitudeDegrees,
                target.LongitudeDegrees) is null) return;
            Camera.FocusForCursorZoom(target, recoverClearance: wheel <= 0f);
        }
    }

    private GeoCoordinate CursorTarget(Vector3 anchor, Vector3 rayHit)
    {
        EnuPosition target = _anchor.ToEnu(Camera.TargetEcef);
        GeoCoordinate moved = Wgs84.ToGeo(_anchor.ToEcef(new EnuPosition(
            target.East + anchor.X - rayHit.X,
            target.North + anchor.Y - rayHit.Y, target.Up)));
        return new GeoCoordinate(moved.LatitudeDegrees,
            moved.LongitudeDegrees, G07EllipsoidHeight(moved, 0d));
    }
}