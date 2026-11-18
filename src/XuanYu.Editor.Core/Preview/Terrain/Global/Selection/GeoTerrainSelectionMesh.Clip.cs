using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static partial class GeoTerrainSelectionMesh
{
    private static void ClipTriangle(List<float> selected, List<float> rest, Vertex a, Vertex b, Vertex c,
        GeoTileBounds bounds)
    {
        // 粗 LOD 三角形可能跨 HGT 边界；切分只为区分轮廓选区，不重采样地形。
        for (int edge = 0; edge < 4; edge++)
            if (Side(a, bounds, edge) < 0 && Side(b, bounds, edge) < 0 &&
                Side(c, bounds, edge) < 0)
            {
                Write(rest, a); Write(rest, b); Write(rest, c);
                return;
            }
        if (Enumerable.Range(0, 4).All(edge => Side(a, bounds, edge) >= 0 &&
            Side(b, bounds, edge) >= 0 && Side(c, bounds, edge) >= 0))
        {
            Write(selected, a); Write(selected, b); Write(selected, c);
            return;
        }
        List<Vertex> inner = [a, b, c];
        for (int edge = 0; edge < 4 && inner.Count > 0; edge++)
        {
            var next = new List<Vertex>(inner.Count + 1);
            var outer = new List<Vertex>(inner.Count + 1);
            Vertex previous = inner[^1];
            double from = Side(previous, bounds, edge);
            foreach (Vertex current in inner)
            {
                double to = Side(current, bounds, edge);
                if ((from < 0) != (to < 0))
                {
                    Vertex cross = Vertex.Lerp(previous, current, from / (from - to));
                    next.Add(cross); outer.Add(cross);
                }
                (to >= 0 ? next : outer).Add(current);
                previous = current;
                from = to;
            }
            WriteFan(rest, outer);
            inner = next;
        }
        WriteFan(selected, inner);
    }

    private static double Side(Vertex v, GeoTileBounds b, int edge)
    {
        double d = edge switch
        {
            0 => v.Latitude - b.SouthLatitudeDegrees,
            1 => b.NorthLatitudeDegrees - v.Latitude,
            2 => v.Longitude - b.WestLongitudeDegrees,
            _ => b.EastLongitudeDegrees - v.Longitude
        };
        // ENU 顶点使用 float；贴边的厘米级尾差不应产生细小误选三角形。
        return Math.Abs(d) < 2e-7d ? 0d : d;
    }

    private static void WriteFan(List<float> output, List<Vertex> polygon)
    {
        for (int i = 1; i + 1 < polygon.Count; i++)
        {
            Vertex a = polygon[0], b = polygon[i], c = polygon[i + 1];
            if (Vector3.Cross(b.Position - a.Position, c.Position - a.Position).LengthSquared() == 0f)
                continue;
            Write(output, a); Write(output, b); Write(output, c);
        }
    }
}