namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedSelector
{
    public GeoUnifiedSelection StepWithMetrics(GeoUnifiedCoverage coverage,
        Func<GeoUnifiedTile, int, double?> screenError, Func<GeoUnifiedTile, int, bool> ready,
        Func<GeoUnifiedTile, bool>? visible = null)
    {
        var parents = coverage.Leaves.Where(tile => tile.Level < 15)
            .Select(tile => tile.Parent()).Distinct().ToArray();
        foreach (var parent in parents)
        {
            if (!coverage.TryCoarsenWithEdges(parent, (_, _) => true, out var candidate)) continue;
            double? error = screenError(parent, candidate.EdgeMask(parent));
            if (error is < 1.5 && coverage.TryCoarsenWithEdges(parent, ready, out var coarse))
                return new(coarse, null, 0);
        }
        var candidates = new List<(GeoUnifiedTile Tile, double Error)>();
        foreach (var tile in coverage.Leaves)
        {
            if (tile.Level == 0 || (visible is not null && !visible(tile))) continue;
            double? error = screenError(tile, coverage.EdgeMask(tile));
            if (error is null || double.IsNaN(error.Value) || error.Value < 0)
                return new(coverage, tile, double.NaN);
            candidates.Add((tile, error.Value));
        }
        foreach (var (tile, error) in candidates.OrderByDescending(item => item.Error))
        {
            if (error <= 2.5) break;
            if (coverage.TryRefineWithEdges(tile, ready, out var refined))
                return new(refined, null, 0);
            return new(coverage, tile, error);
        }
        return new(coverage, null, 0);
    }
}
