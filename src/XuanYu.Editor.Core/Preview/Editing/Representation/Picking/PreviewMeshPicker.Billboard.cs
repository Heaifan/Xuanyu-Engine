using System.Numerics;
using XuanYu.Runtime;

namespace XuanYu.Editor.Preview;

public static partial class PreviewMeshPicker
{
    internal static Guid? PickBillboard(PreviewScene scene, Vector2 pixel, uint width, uint height,
        Matrix4x4 projection, GeoRenderFrame? frame = null)
    {
        Guid? selected = null;
        float nearest = 1;
        foreach (var item in scene.Objects)
        {
            if (!item.Visible || item.IsGeographic != (frame is not null) ||
                !WorldRenderBridge.IsBillboard(scene.GetRepresentation(item.Id)) ||
                scene.GetActiveMesh(item.Id) is not { } mesh) continue;
            if (frame is not null && !frame.IsAboveHorizon(scene.World.GetSpatialPosition(item.Id).ToEcef())) continue;
            var matrix = frame is null ? scene.LocalDrawMatrix(item.Id, projection, width, height)
                : scene.GeographicDrawMatrix(item.Id, frame, width, height);
            if (!VisibleBillboard(matrix)) continue;
            float depth = matrix.M43/matrix.M44;
            if (depth > nearest || !Hits(mesh, pixel, matrix, width, height)) continue;
            nearest = depth;
            selected = item.Id;
        }
        return selected;
    }
}