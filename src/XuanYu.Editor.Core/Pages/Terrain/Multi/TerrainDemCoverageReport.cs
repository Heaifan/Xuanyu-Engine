namespace XuanYu.Editor.Pages;

public sealed record TerrainDemCoverageReport(int MissingCount, IReadOnlyList<string> Holes)
{
    public static TerrainDemCoverageReport Check(IEnumerable<(int South, int West)> coordinates)
    {
        var loaded = coordinates.ToHashSet();
        if (loaded.Count == 0) return new(0, []);
        int south = loaded.Min(p => p.South), north = loaded.Max(p => p.South);
        int west = loaded.Min(p => p.West), east = loaded.Max(p => p.West);
        var missing = new HashSet<(int South, int West)>();
        for (int y = south; y <= north; y++)
            for (int x = west; x <= east; x++)
                if (!loaded.Contains((y, x))) missing.Add((y, x));
        int count = missing.Count;
        var exterior = new Queue<(int South, int West)>(missing.Where(p =>
            p.South == south || p.South == north || p.West == west || p.West == east));
        foreach (var boundary in exterior) missing.Remove(boundary);
        while (exterior.TryDequeue(out var p))
        {
            foreach (var next in new[] { (p.South - 1, p.West), (p.South + 1, p.West),
                (p.South, p.West - 1), (p.South, p.West + 1) })
                if (missing.Remove(next)) exterior.Enqueue(next);
        }
        return new(count, missing.OrderBy(p => p.South).ThenBy(p => p.West)
            .Select(p => TileId(p.South, p.West)).ToArray());
    }

    private static string TileId(int south, int west) =>
        $"{(south < 0 ? 'S' : 'N')}{Math.Abs(south):00}{(west < 0 ? 'W' : 'E')}{Math.Abs(west):000}";
}