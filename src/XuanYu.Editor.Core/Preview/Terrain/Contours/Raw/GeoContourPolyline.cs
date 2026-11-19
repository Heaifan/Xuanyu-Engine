using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class GeoContourPolyline
{
    private readonly record struct Key(long Y, long X, int Level);

    public static RenderMesh? Build(GeoTileId tile, IReadOnlyList<GeoContourSegment> segments,
        EnuFrame anchor, GeoContourStyle style)
    {
        if (segments.Count == 0) return null;
        var ends = new Dictionary<Key, List<int>>();
        for (int i = 0; i < segments.Count; i++)
        {
            Index(ends, KeyOf(segments[i].A, segments[i].Level), i);
            Index(ends, KeyOf(segments[i].B, segments[i].Level), i);
        }
        var visited = new bool[segments.Count];
        var output = new List<float>();
        for (int i = 0; i < segments.Count; i++)
        {
            if (visited[i]) continue;
            visited[i] = true;
            var path = new LinkedList<GeoContourCoordinate>();
            path.AddLast(segments[i].A);
            path.AddLast(segments[i].B);
            Extend(path, segments[i].Level, true, ends, segments, visited);
            Extend(path, segments[i].Level, false, ends, segments, visited);
            GeoContourRibbon.Append(output, path.ToList(), segments[i].Level, anchor, style);
        }
        return output.Count == 0 ? null : new RenderMesh(
            $"terrain/contour/{tile}/{style.IntervalMeters}/{style.WidthMeters}", output.ToArray());
    }

    private static void Index(Dictionary<Key, List<int>> ends, Key key, int index)
    {
        if (!ends.TryGetValue(key, out var ids)) ends[key] = ids = [];
        ids.Add(index);
    }

    private static Key KeyOf(GeoContourCoordinate p, int level) =>
        new((long)Math.Round(p.Latitude * 1e9d),
            (long)Math.Round(p.Longitude * 1e9d), level);

    private static void Extend(LinkedList<GeoContourCoordinate> path, int level,
        bool forward, Dictionary<Key, List<int>> ends,
        IReadOnlyList<GeoContourSegment> segments, bool[] visited)
    {
        while (true)
        {
            GeoContourCoordinate end = forward ? path.Last!.Value : path.First!.Value;
            Key key = KeyOf(end, level);
            if (!ends.TryGetValue(key, out var ids)) break;
            int next = -1;
            foreach (int index in ids)
                if (!visited[index]) { next = index; break; }
            if (next < 0) break;
            visited[next] = true;
            var segment = segments[next];
            GeoContourCoordinate other = KeyOf(segment.A, level) == key
                ? segment.B : segment.A;
            if (forward) path.AddLast(other);
            else path.AddFirst(other);
            if (KeyOf(path.First!.Value, level) == KeyOf(path.Last!.Value, level)) break;
        }
    }
}