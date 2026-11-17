using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public void ResetCamera()
    {
        Camera = new GlobalCamera(_anchor.Origin, 150_000d);
        Camera.SetTerrainCollision(SampleGroundHeight);
    }

    public void Navigate(float dx, float dy, float wheel, bool pan,
        uint viewportHeight)
    {
        if (wheel != 0f)
            Camera.Zoom(wheel);
        else if (pan)
        {
            double meters = 2d * Camera.DistanceMeters * Math.Tan(Math.PI / 6d) /
                Math.Max(1u, viewportHeight);
            double yaw = Camera.YawRadians;
            double east = (-dx * Math.Cos(yaw) + dy * Math.Sin(yaw)) * meters;
            double north = (-dx * Math.Sin(yaw) - dy * Math.Cos(yaw)) * meters;
            var origin = new EnuFrame(Camera.Target);
            GeoCoordinate geo = Wgs84.ToGeo(origin.ToEcef(
                new EnuPosition(east, north, 0d)));
            Camera.Focus(new GeoCoordinate(geo.LatitudeDegrees,
                geo.LongitudeDegrees, 0d));
        }
        else
            Camera.Orbit(-dx * 0.006d, dy * 0.006d);
    }

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
            var origin = new EnuFrame(Camera.Target);
            GeoCoordinate position = Wgs84.ToGeo(origin.ToEcef(new EnuPosition(
                anchor.X - atHeight.X, anchor.Y - atHeight.Y, 0d)));
            Camera.FocusForCursorZoom(new GeoCoordinate(position.LatitudeDegrees,
                position.LongitudeDegrees, 0d));
        }
    }

    private Matrix4x4 ViewProjection(uint width, uint height)
    {
        EnuPosition eye = _anchor.ToEnu(Camera.PositionEcef);
        EnuPosition target = _anchor.ToEnu(Camera.TargetEcef);
        Matrix4x4 view = Matrix4x4.CreateLookAt(ToFloat(eye),
            ToFloat(target), Vector3.UnitZ);
        float near = (float)Math.Max(0.1d, Camera.DistanceMeters * 0.01d);
        float far = (float)Math.Max(500_000d, Camera.DistanceMeters * 8d);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 3f, (float)width / height, near, far);
        projection.M22 = -projection.M22;
        return view * projection;
    }

    private static Vector3 ToFloat(EnuPosition p) =>
        new((float)p.East, (float)p.North, (float)p.Up);
}