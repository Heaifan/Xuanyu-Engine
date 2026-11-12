using System.Numerics;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.Camera;

public static class ViewportSceneFraming
{
    public static bool FrameAll(ViewportCamera camera,
        IEnumerable<PreviewObject> objects, float aspect)
    {
        camera.Reset();
        return Fit(camera, objects, aspect);
    }

    public static bool Focus(ViewportCamera camera,
        PreviewObject? selected, float aspect) =>
        selected is not null && Fit(camera, [selected], aspect);

    private static bool Fit(ViewportCamera camera,
        IEnumerable<PreviewObject> objects, float aspect)
    {
        Vector3 min = new(float.PositiveInfinity);
        Vector3 max = new(float.NegativeInfinity);

        foreach (PreviewObject item in objects)
        {
            if (!item.Visible)
                continue;
            Matrix4x4 modelToWorld = item.ModelToClip;
            ReadOnlySpan<float> vertices = item.Mesh.Vertices.Span;
            for (int i = 0; i < vertices.Length; i += 7)
            {
                Vector3 local = new(vertices[i], vertices[i + 1],
                    vertices[i + 2]);
                Vector3 world = Vector3.Transform(local, modelToWorld);
                min = Vector3.Min(min, world);
                max = Vector3.Max(max, world);
            }
        }

        if (float.IsPositiveInfinity(min.X))
            return false;

        camera.Frame((min + max) * 0.5f,
            Vector3.Distance(min, max) * 0.5f, aspect);
        return true;
    }
}