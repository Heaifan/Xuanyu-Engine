using System.Numerics;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportNavigationPanel
{
    private GlobalCamera? GeoCamera() => _workbench is { } wb &&
        wb.IsGeoTerrainPreview && !wb.IsSimulating ? wb.GeoTerrainCamera : null;

    private ViewportNavPose? Pose()
    {
        if (GeoCamera() is { } geo)
        {
            double yaw = geo.YawRadians, pitch = geo.PitchRadians;
            var outward = new Vector3(
                (float)(Math.Cos(pitch) * Math.Sin(yaw)),
                (float)(-Math.Cos(pitch) * Math.Cos(yaw)),
                (float)Math.Sin(pitch));
            var right = new Vector3((float)Math.Cos(yaw),
                (float)Math.Sin(yaw), 0f);
            return new ViewportNavPose(outward, right,
                Vector3.Normalize(Vector3.Cross(right, -outward)));
        }
        if (Camera() is not { } camera) return null;
        return new ViewportNavPose(
            Vector3.Normalize(camera.Position - camera.Target),
            camera.ViewRight, camera.ViewUp);
    }

    private void ZoomActive(float steps)
    {
        if (GeoCamera() is { } geo) geo.Zoom(steps);
        else Camera()?.Zoom(steps);
    }

    private void PanActive(float dx, float dy)
    {
        uint height = Math.Max(1u, _viewportSize!().Height);
        if (GeoCamera() is not null)
            _workbench!.NavigateGeoTerrain(dx, dy, 0f, true, height);
        else Camera()?.Pan(dx, dy, height);
    }

    private void OrbitActive(float dx, float dy)
    {
        if (GeoCamera() is not null)
            _workbench!.NavigateGeoTerrain(dx, dy, 0f, false, 1);
        else Camera()?.Orbit(dx, dy);
    }

    private void SnapActive(Vector3 axis)
    {
        if (GeoCamera() is not null)
        {
            var (width, height) = _viewportSize!.Invoke();
            _workbench!.SnapGeoTerrainAxis(axis, width, height);
        }
        else Camera()?.SnapAxis(axis);
    }
}