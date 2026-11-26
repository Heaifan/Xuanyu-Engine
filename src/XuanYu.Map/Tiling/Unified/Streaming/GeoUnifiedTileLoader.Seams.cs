namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedTileLoader
{
    private readonly Dictionary<(string, string), GeoUnifiedSeamReport> _seams = [];

    public IReadOnlyList<GeoUnifiedSeamReport> SeamReports
    {
        get { lock (_gate) return _seams.Values.ToArray(); }
    }

    private void RecordSeams(NasademTile tile)
    {
        int south = tile.SouthLatitude, west = tile.WestLongitude;
        (int, int)[] neighbors = [(south - 1, west), (south + 1, west),
            (south, west == -180 ? 179 : west - 1), (south, west == 179 ? -180 : west + 1)];
        foreach (var key in neighbors)
        {
            if (!_requests.TryGetValue(key, out var source) ||
                !source.Task.IsCompletedSuccessfully || source.Task.Result is not { } other) continue;
            GeoUnifiedSeamReport report = GeoUnifiedSeamAudit.Compare(tile, other);
            var id = string.CompareOrdinal(report.First, report.Second) < 0
                ? (report.First, report.Second) : (report.Second, report.First);
            if (report.Conflicts > 0) _seams[id] = report;
        }
    }
}
