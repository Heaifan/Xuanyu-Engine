using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public static class PreviewCubeMesh
{
    public static RenderMesh Create()
    {
        (Vector3 normal, Vector3 right, Vector3 up, Vector4 color)[] faces =
        [
            (Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ,
                new Vector4(0.95f, 0.35f, 0.25f, 1f)),
            (-Vector3.UnitX, -Vector3.UnitY, Vector3.UnitZ,
                new Vector4(0.65f, 0.28f, 0.25f, 1f)),
            (Vector3.UnitY, -Vector3.UnitX, Vector3.UnitZ,
                new Vector4(0.25f, 0.75f, 0.9f, 1f)),
            (-Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ,
                new Vector4(0.22f, 0.55f, 0.75f, 1f)),
            (Vector3.UnitZ, Vector3.UnitX, Vector3.UnitY,
                new Vector4(0.95f, 0.85f, 0.35f, 1f)),
            (-Vector3.UnitZ, Vector3.UnitX, -Vector3.UnitY,
                new Vector4(0.45f, 0.65f, 0.3f, 1f))
        ];
        List<float> vertices = new(36 * 7);
        foreach (var (normal, right, up, color) in faces)
        {
            Vector3 center = normal * 0.7f;
            Vector3 r = right * 0.7f;
            Vector3 u = up * 0.7f;
            Vector3 a = center - r - u;
            Vector3 b = center + r - u;
            Vector3 c = center + r + u;
            Vector3 d = center - r + u;
            AddVertex(vertices, a, color);
            AddVertex(vertices, b, color);
            AddVertex(vertices, c, color);
            AddVertex(vertices, a, color);
            AddVertex(vertices, c, color);
            AddVertex(vertices, d, color);
        }
        return new RenderMesh("builtin/cube", vertices.ToArray());
    }

    private static void AddVertex(List<float> values, Vector3 p, Vector4 color)
    {
        values.AddRange([p.X, p.Y, p.Z,
            color.X, color.Y, color.Z, color.W]);
    }
}
