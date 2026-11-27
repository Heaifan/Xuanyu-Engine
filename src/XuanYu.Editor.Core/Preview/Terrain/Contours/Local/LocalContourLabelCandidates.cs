using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class LocalContourLabelCandidates
{
    public static GeoContourLabel[] Build(RenderDraw draw, TerrainMeshGeometry geometry,
        int interval, uint width, uint height)
    {
        if (interval <= 0 || width < 200 || height < 150) return [];
        var p = geometry.Positions;
        double min = p.Min(v => v.Z), max = p.Max(v => v.Z);
        if (min == max || min < int.MinValue || max > int.MaxValue) return [];
        // 只稀疏标注计曲线，不改变 GPU 实际绘制的等高距。
        double step = interval * 5d;
        step *= Math.Max(1d, Math.Ceiling((max - min) / (step * 64d)));
        var topology = new GeoContourLabelTopology();
        var candidates = new Dictionary<(int X, int Y), (GeoContourGridEdge Edge,
            GeoContourLabel Label, float Length)>();
        var indices = geometry.Indices;
        for (int i = 0; i < indices.Length; i += 3)
        {
            int a = indices[i], b = indices[i + 1], c = indices[i + 2];
            double low = Math.Min(p[a].Z, Math.Min(p[b].Z, p[c].Z));
            double high = Math.Max(p[a].Z, Math.Max(p[b].Z, p[c].Z));
            for (double level = Math.Ceiling(low / step) * step; level < high; level += step)
                Trace(a, b, c, (int)level, p, draw, width, height, topology, candidates);
        }
        foreach (var entry in candidates.Values) topology.Add(entry.Edge, entry.Label);
        return GeoContourLabelLayout.Select(topology.Candidates(), width, height);
    }

    private static void Trace(int a, int b, int c, int level, Vector3[] positions,
        RenderDraw draw, uint width, uint height, GeoContourLabelTopology topology,
        Dictionary<(int X, int Y), (GeoContourGridEdge Edge, GeoContourLabel Label,
            float Length)> candidates)
    {
        Span<int> ids = stackalloc int[3] { a, b, c };
        Span<GeoContourGridEdge> keys = stackalloc GeoContourGridEdge[3];
        Span<Vector3> cuts = stackalloc Vector3[3];
        int count = 0;
        for (int i = 0; i < 3; i++)
        {
            int u = ids[i], v = ids[(i + 1) % 3];
            Vector3 pa = positions[u], pb = positions[v];
            if (!GeoContourMesh.Cross(new(pa, Vector3.Zero, pa.Z),
                new(pb, Vector3.Zero, pb.Z), level, out var cut)) continue;
            // 共享边由顶点索引确定；恰好穿过顶点时合并成同一个节点。
            if (pa.Z == level) v = u;
            else if (pb.Z == level) u = v;
            keys[count] = new(Math.Min(u, v), Math.Max(u, v), false, level);
            cuts[count++] = cut.Position;
        }
        if (count != 2 || keys[0] == keys[1]) return;
        Vector3 delta = cuts[1] - cuts[0];
        float length = delta.LengthSquared();
        if (length <= 1e-12f) return;
        topology.Add(keys[0], null);
        topology.Add(keys[1], null);
        topology.Join(keys[0], keys[1]);
        Vector3 center = (cuts[0] + cuts[1]) * 0.5f;
        Vector3 tangent = delta / 40f; // TryScreen 的 ±20 对应本段两个端点。
        var label = new GeoContourLabel(0, 0, level, 0, center.X, center.Y, level,
            tangent.X, tangent.Y, 0, 0) { Owner = draw.Mesh, TangentUp = tangent.Z };
        if (!GeoContourLabelProjection.TryScreen(label, draw.ModelToClip, width, height,
            out label)) return;
        var bucket = ((int)(label.X / 36f), (int)(label.Y / 24f));
        if (!candidates.TryGetValue(bucket, out var old) || old.Length < length)
            candidates[bucket] = (keys[0], label, length);
    }
}