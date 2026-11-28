using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Runtime;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview;

public static class GeoPreviewMeshPicker
{
    public static Guid? Pick(PreviewScene scene, GeoUnifiedLodSession cpu,
        GeoRenderFrame frame, Vector2 pixel, uint width, uint height)
    {
        if (!ViewportScreenRay.TryCreate(frame.RelativeProjection, pixel, width, height,
            out var start, out var ray)) return null;
        Guid? selected = null;
        float closest = 1f;
        foreach (var item in scene.Objects)
        {
            if (!item.Visible || !item.IsGeographic) continue;
            var model = frame.ModelToClip(scene.World.Find(item.Id)!.Transform);
            if (!ViewportScreenRay.TryCreate(model, pixel, width, height,
                out var localStart, out var localRay)) continue;
            var data = item.Mesh.Vertices.Span;
            for (int i = 0; i < data.Length; i += 21)
            {
                if (!Intersect(localStart, localRay, Vertex(data, i), Vertex(data, i + 7),
                    Vertex(data, i + 14), out float t)) continue;
                float depth = Depth(localStart + localRay * t, model);
                if (depth < 0f || depth >= closest || !float.IsFinite(depth)) continue;
                closest = depth;
                selected = item.Id;
            }
        }
        if (selected is not null && cpu.TryPickSurfaceHit(start, ray, out var hit, frame.Eye) &&
            Depth(hit.Position, frame.RelativeProjection) <= closest) return null;
        return selected;
    }

    private static Vector3 Vertex(ReadOnlySpan<float> data, int i) =>
        new(data[i], data[i + 1], data[i + 2]);

    private static float Depth(Vector3 point, Matrix4x4 projection)
    {
        var clip = Vector4.Transform(new Vector4(point, 1f), projection);
        return clip.W > 0f ? clip.Z / clip.W : float.PositiveInfinity;
    }

    private static bool Intersect(Vector3 origin, Vector3 ray, Vector3 a,
        Vector3 b, Vector3 c, out float distance)
    {
        distance = 0f;
        Vector3 ab = b - a, ac = c - a;
        Vector3 p = Vector3.Cross(ray, ac);
        float det = Vector3.Dot(ab, p);
        if (MathF.Abs(det) < 1e-7f) return false;
        Vector3 offset = origin - a;
        float u = Vector3.Dot(offset, p) / det;
        if (u < 0f || u > 1f) return false;
        Vector3 q = Vector3.Cross(offset, ab);
        float v = Vector3.Dot(ray, q) / det;
        if (v < 0f || u + v > 1f) return false;
        distance = Vector3.Dot(ac, q) / det;
        return distance >= 0f;
    }
}