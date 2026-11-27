namespace XuanYu.World.Maps;

public readonly record struct GeoUnifiedSelection(
    GeoUnifiedCoverage Coverage, GeoUnifiedTile? BlockedTile, double BlockedPixels)
{
    public GeoUnifiedCoverage? PendingCoverage { get; init; }
    public bool CapacityLimited { get; init; }
    public string? BudgetDiagnostic { get; init; }
}

public sealed partial class GeoUnifiedSelector
{
    public GeoUnifiedSelection Step(GeoUnifiedCoverage coverage,
        Func<GeoUnifiedTile, double> screenError, Func<GeoUnifiedTile, bool> ready,
        Func<GeoUnifiedTile, bool>? visible = null) =>
        StepWithEdges(coverage, screenError, (tile, _) => ready(tile), visible);

    public GeoUnifiedSelection StepWithEdges(GeoUnifiedCoverage coverage,
        Func<GeoUnifiedTile, double> screenError, Func<GeoUnifiedTile, int, bool> ready,
        Func<GeoUnifiedTile, bool>? visible = null)
    {
        var parents = coverage.Leaves.Where(tile => tile.Level < 15)
            .Select(tile => tile.Parent()).Distinct()
            .Where(parent => coverage.Leaves.Contains(parent.Child(0, 0)) &&
                coverage.Leaves.Contains(parent.Child(1, 0)) &&
                coverage.Leaves.Contains(parent.Child(0, 1)) &&
                coverage.Leaves.Contains(parent.Child(1, 1))).ToArray();
        foreach (var parent in parents.OrderBy(screenError))
        {
            double error = screenError(parent);
            if (error < 1.5 && coverage.TryCoarsenWithEdges(parent, ready, out var coarse))
                return new(coarse, null, 0);
        }
        foreach (var tile in coverage.Leaves.Where(tile => tile.Level > 0 && (visible is null || visible(tile)))
            .OrderByDescending(screenError))
        {
            double error = screenError(tile);
            if (!GeoUnifiedScreenError.ShouldRefine(error, false)) break;
            return Refine(coverage, tile, error, ready);
        }
        return new(coverage, null, 0);
    }

    private static GeoUnifiedSelection Refine(GeoUnifiedCoverage coverage, GeoUnifiedTile tile,
        double error, Func<GeoUnifiedTile, int, bool> ready)
    {
        var candidate = coverage.PrepareBalancedRefinement(tile);
        if (candidate.Leaves.Any(leaf => !ready(leaf, candidate.EdgeMask(leaf))))
            return new(coverage, tile, error) { PendingCoverage = candidate };
        return new(candidate, null, 0);
    }
}