using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class LocalTerrainMeshUpdate
{
    public static RenderMesh? Apply(TerrainHeightField field, TerrainMeshGeometry geometry,
        RenderMesh previous)
    {
        int w = field.SamplesX, h = field.SamplesY;
        int left = w, right = -1, bottom = h, top = -1;
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int i = y * w + x;
            float z = field.GetHeight(x, y);
            if (geometry.Positions[i].Z == z) continue;
            geometry.Positions[i].Z = z;
            left = Math.Min(left, x); right = Math.Max(right, x);
            bottom = Math.Min(bottom, y); top = Math.Max(top, y);
        }
        if (right < 0) return null;
        left = Math.Max(0, left - 1); right = Math.Min(w - 1, right + 1);
        bottom = Math.Max(0, bottom - 1); top = Math.Min(h - 1, top + 1);
        var colors = new GeoTerrainEdgeVertex[(right - left + 1) * (top - bottom + 1)];
        int stride = right - left + 1;
        for (int y = bottom; y <= top; y++)
        for (int x = left; x <= right; x++)
        {
            int i = y * w + x;
            geometry.Normals[i] = NormalAt(geometry, w, h, x, y);
            Vector3 p = geometry.Positions[i];
            colors[(y - bottom) * stride + x - left] =
                GeoTerrainEdgeVertex.FromHeight(p, p.Z, geometry.Normals[i]);
        }
        // 兼容仍由旧 FromLocalTerrain 重载创建的单色通道网格。
        if (previous.TerrainShadedColors.IsEmpty)
            return TerrainRenderMeshAdapter.CreateLocal(previous.AssetId, geometry);
        var data = previous.Vertices.ToArray();
        var shaded = previous.TerrainShadedColors.ToArray();
        for (int y = Math.Max(0, bottom - 1); y <= Math.Min(h - 2, top); y++)
        for (int x = Math.Max(0, left - 1); x <= Math.Min(w - 2, right); x++)
        {
            int start = (y * (w - 1) + x) * 6;
            for (int j = start; j < start + 6; j++)
            {
                int i = geometry.Indices[j], vx = i % w, vy = i / w;
                if (vx < left || vx > right || vy < bottom || vy > top) continue;
                var vertex = colors[(vy - bottom) * stride + vx - left];
                Vector3 p = vertex.Position, c = vertex.Color, light = vertex.ShadedColor;
                int o = j * 7, q = j * 3;
                data[o] = p.X; data[o + 1] = p.Y; data[o + 2] = p.Z;
                data[o + 3] = c.X; data[o + 4] = c.Y; data[o + 5] = c.Z;
                shaded[q] = light.X; shaded[q + 1] = light.Y; shaded[q + 2] = light.Z;
            }
        }
        return RenderMesh.FromLocalTerrain(previous.AssetId, data, shaded);
    }

    private static Vector3 NormalAt(TerrainMeshGeometry geometry, int w, int h, int x, int y)
    {
        int vertex = y * w + x;
        Vector3 normal = Vector3.Zero;
        for (int cy = Math.Max(0, y - 1); cy <= Math.Min(h - 2, y); cy++)
        for (int cx = Math.Max(0, x - 1); cx <= Math.Min(w - 2, x); cx++)
        {
            int start = (cy * (w - 1) + cx) * 6;
            for (int j = start; j < start + 6; j += 3)
            {
                int a = geometry.Indices[j], b = geometry.Indices[j + 1], c = geometry.Indices[j + 2];
                if (a != vertex && b != vertex && c != vertex) continue;
                var p = geometry.Positions;
                normal += Vector3.Cross(p[b] - p[a], p[c] - p[a]);
            }
        }
        return Vector3.Normalize(normal);
    }
}