namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedSelector
{
    public GeoUnifiedSelection StepWithMetrics(GeoUnifiedCoverage coverage,
        Func<GeoUnifiedTile, int, double?> screenError, Func<GeoUnifiedTile, int, bool> ready,
        Func<GeoUnifiedTile, bool>? visible = null)
    {
        var parents = coverage.Leaves.Where(tile => tile.Level < 15)
            .Select(tile => tile.Parent()).Distinct();
        foreach (var parent in parents)
        {
            if (!coverage.CanCoarsen(parent)) continue;
            double? error = screenError(parent, coverage.CoarseNeighborMask(parent));
            if (error is not < 1.5) continue;
            if (!coverage.TryCoarsen(parent, _ => true, out var candidate)) continue;
            if (candidate.Leaves.All(tile => ready(tile, candidate.EdgeMask(tile))))
                return new(candidate, null, 0);
            return new(coverage, parent, error.Value) { PendingCoverage = candidate };
        }
        GeoUnifiedTile? best = null;
        double highest = 2.5;
        foreach (var tile in coverage.Leaves)
        {
            if (tile.Level == 0 || (visible is not null && !visible(tile))) continue;
            double? error = screenError(tile, coverage.EdgeMask(tile));
            if (error is null || double.IsNaN(error.Value) || error.Value < 0)
                return new(coverage, tile, double.NaN);
            if (error.Value <= highest) continue;
            highest = error.Value;
            best = tile;
        }
        return best is { } target ? Refine(coverage, target, highest, ready) : new(coverage, null, 0);
    }
}