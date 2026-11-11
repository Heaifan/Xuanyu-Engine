using System.Numerics;

namespace XuanYu.Editor.Preview;

public static class PreviewMeshPicker
{
    public static Guid? Pick(PreviewScene scene, int x, int y,
        uint width, uint height)
    {
        Vector2 point = new(x, y);
        Matrix4x4 camera = scene.ViewProjection(width, height);
        for (int i = scene.Objects.Count - 1; i >= 0; i--)
        {
            PreviewObject item = scene.Objects[i];
            if (!item.Visible)
                continue;
            Matrix4x4 transform = item.ModelToClip * camera;
            ReadOnlySpan<float> data = item.Mesh.Vertices.Span;
            for (int offset = 0; offset < data.Length; offset += 21)
            {
                Vector2 a = Project(data, offset, transform, width, height);
                Vector2 b = Project(data, offset + 7, transform, width, height);
                Vector2 c = Project(data, offset + 14, transform, width, height);
                if (Contains(point, a, b, c))
                    return item.Id;
            }
        }
        return null;
    }

    private static Vector2 Project(ReadOnlySpan<float> data, int offset,
        Matrix4x4 matrix, uint width, uint height)
    {
        Vector4 clip = Vector4.Transform(new Vector4(
            data[offset], data[offset + 1], data[offset + 2], 1f), matrix);
        return new Vector2(
            (clip.X / clip.W * 0.5f + 0.5f) * width,
            (clip.Y / clip.W * 0.5f + 0.5f) * height);
    }

    private static bool Contains(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Edge(a, b, c);
        if (MathF.Abs(area) < 0.0001f)
            return false;
        float ab = Edge(a, b, p);
        float bc = Edge(b, c, p);
        float ca = Edge(c, a, p);
        return (ab >= 0 && bc >= 0 && ca >= 0) ||
               (ab <= 0 && bc <= 0 && ca <= 0);
    }

    private static float Edge(Vector2 a, Vector2 b, Vector2 p) =>
        (b.X - a.X) * (p.Y - a.Y) -
        (b.Y - a.Y) * (p.X - a.X);
}
