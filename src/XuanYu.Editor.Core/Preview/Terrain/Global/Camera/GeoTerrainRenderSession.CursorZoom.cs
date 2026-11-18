using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public void ZoomAtCursor(float wheel, Vector2 pixel, uint width, uint height)
    {
        if (width < 2 || height < 2) return;
        bool found = TryPickZoomAnchor(pixel, width, height, out Vector3 anchor);
        double before = Camera.DistanceMeters;
        Camera.Zoom(wheel);
        if (!found || Camera.DistanceMeters == before) return;

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
            Camera.FocusForCursorZoom(target);
        }
    }

    private GeoCoordinate CursorTarget(Vector3 anchor, Vector3 rayHit)
    {
        EnuPosition target = _anchor.ToEnu(Camera.TargetEcef);
        GeoCoordinate moved = Wgs84.ToGeo(_anchor.ToEcef(new EnuPosition(
            target.East + anchor.X - rayHit.X,
            target.North + anchor.Y - rayHit.Y, target.Up)));
        return new GeoCoordinate(moved.LatitudeDegrees,
            moved.LongitudeDegrees, 0d);
    }
}