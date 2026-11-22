using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoContourLabelSurface
{
    public static bool TryLocate(RenderDraw draw, Vector2 uv, Vector2 tangentUv,
        out Vector3 position, out Vector3 tangent)
    {
        position = tangent = default;
        var vertices = draw.Mesh.Vertices.Span;
        var uvs = draw.DemUV.Span;
        if (uvs.Length != draw.Mesh.VertexCount * 2) return false;
        if (draw.WaterAtlas is { } water && draw.ResidentDem is { } dem)
            uv += new Vector2(dem.WestLongitude - water.WestLongitude,
                water.SouthLatitude - dem.SouthLatitude);
        for (int i = 0; i < draw.Mesh.VertexCount; i += 3)
        {
            Vector2 a = Uv(uvs, i), b = Uv(uvs, i + 1), c = Uv(uvs, i + 2);
            Vector2 ab = b - a, ac = c - a, q = uv - a;
            float det = Cross(ab, ac);
            if (MathF.Abs(det) < 1e-15f) continue;
            float u = Cross(q, ac) / det, v = Cross(ab, q) / det;
            if (u < -1e-5f || v < -1e-5f || u + v > 1.00001f) continue;
            Vector3 pa = Vertex(vertices, i), pb = Vertex(vertices, i + 1);
            Vector3 pc = Vertex(vertices, i + 2);
            position = pa + u * (pb - pa) + v * (pc - pa);
            float du = Cross(tangentUv, ac) / det, dv = Cross(ab, tangentUv) / det;
            tangent = du * (pb - pa) + dv * (pc - pa);
            if (tangent.LengthSquared() < 1e-12f) return false;
            tangent = Vector3.Normalize(tangent);
            return true;
        }
        return false;
    }

    private static Vector2 Uv(ReadOnlySpan<float> data, int i) =>
        new(data[i * 2], data[i * 2 + 1]);
    private static Vector3 Vertex(ReadOnlySpan<float> data, int i) =>
        new(data[i * 7], data[i * 7 + 1], data[i * 7 + 2]);
    private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
}
