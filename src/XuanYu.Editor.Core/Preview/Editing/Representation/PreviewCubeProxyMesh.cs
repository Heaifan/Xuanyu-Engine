using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

internal static class PreviewCubeProxyMesh
{
    public static RenderMesh Create()
    {
        const float r = 0.7f;
        Vector3[] ring = [new(r, 0, 0), new(0, r, 0),
            new(-r, 0, 0), new(0, -r, 0)];
        var top = new Vector3(0, 0, r);
        var bottom = new Vector3(0, 0, -r);
        var vertices = new List<float>(8 * 21);
        for (int i = 0; i < ring.Length; i++)
        {
            var a = ring[i];
            var b = ring[(i + 1) % ring.Length];
            AddTriangle(vertices, a, b, top, new(0.25f, 0.72f, 0.95f));
            AddTriangle(vertices, b, a, bottom, new(0.16f, 0.43f, 0.68f));
        }
        return new RenderMesh("builtin/cube-proxy", vertices.ToArray());
    }

    private static void AddTriangle(List<float> vertices,
        Vector3 a, Vector3 b, Vector3 c, Vector3 color)
    {
        foreach (var p in new[] { a, b, c })
            vertices.AddRange([p.X, p.Y, p.Z, color.X, color.Y, color.Z, 1f]);
    }
}