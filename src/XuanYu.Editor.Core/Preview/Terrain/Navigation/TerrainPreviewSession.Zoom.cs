using System.Numerics;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    public void ZoomAtCursor(float wheel, Vector2 pixel, uint width, uint height)
    {
        if (width < 2 || height < 2 || Scene.Camera is not { } camera) return;
        bool found = TryPick(pixel, width, height, out var hit);
        Vector3 anchor = found
            ? (hit.WorldPosition - new Vector3(OriginMeters, 0f)) * DisplayScale
            : default;
        Vector3 previous = camera.Position;
        camera.Zoom(wheel);
        if (!found || camera.Position == previous) return;

        for (int i = 0; i < 2; i++)
        {
            if (!ViewportScreenRay.TryCreate(camera.ViewProjection(width, height),
                pixel, width, height, out Vector3 start, out Vector3 ray) ||
                MathF.Abs(ray.Z) < 1e-4f) return;
            float distance = (anchor.Z - start.Z) / ray.Z;
            if (!float.IsFinite(distance) || distance <= 0f) return;
            Vector3 atHeight = start + ray * distance;
            Vector3 shift = anchor - atHeight;
            camera.Focus(camera.Target + new Vector3(shift.X, shift.Y, 0f));
        }
    }
}