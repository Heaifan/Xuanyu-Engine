namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedSelector
{
    public GeoUnifiedSelection StepWithMetrics(GeoUnifiedCoverage coverage,
        Func<GeoUnifiedTile, int, double?> screenError, Func<GeoUnifiedTile, int, bool> ready,
        Func<GeoUnifiedTile, bool>? visible = null, int capacity = int.MaxValue, int transitionReserve = 0)
    {
        int steadyLimit = capacity - transitionReserve;
        var parents = coverage.Leaves.Where(tile => tile.Level < 15)
            .Select(tile => tile.Parent()).Distinct();
        var donors = new List<(GeoUnifiedTile Tile, double Error)>();
        int legal = 0, unavailable = 0;
        foreach (var parent in parents)
        {
            if (!coverage.CanCoarsen(parent)) continue;
            legal++;
            double? error = screenError(parent, coverage.CoarseNeighborMask(parent));
            if (error is null || !double.IsFinite(error.Value) || error.Value < 0)
            { unavailable++; continue; }
            if (coverage.Leaves.Count >= capacity) continue;
            if (error.Value >= 1.5)
            {
                if (capacity != int.MaxValue) donors.Add((parent, error.Value));
                continue;
            }
            if (coverage.TryCoarsen(parent, _ => true, out var coarse))
                return Publish(coverage, coarse, parent, error.Value, ready);
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
        if (best is not { } target) return new(coverage, null, 0);
        var candidate = coverage.PrepareBalancedRefinement(target);
        int required = coverage.TransitionPageCount(candidate);
        if (required <= capacity && candidate.Leaves.Count <= steadyLimit)
            return Publish(coverage, candidate, target, highest, ready);
        string budget = $"稳态={coverage.Leaves.Count}/{steadyLimit} 候选={candidate.Leaves.Count} " +
            $"过渡={required}/{capacity} 目标=L{target.Level}/{target.X}/{target.Y} SSE={highest:F2}";
        return RecoverCapacity(coverage, target, highest, required, donors, ready,
            $"{budget} 合法父页={legal} 指标不可用={unavailable}");
    }

    private static GeoUnifiedSelection RecoverCapacity(GeoUnifiedCoverage coverage,
        GeoUnifiedTile target, double error, int required,
        List<(GeoUnifiedTile Tile, double Error)> donors, Func<GeoUnifiedTile, int, bool> ready,
        string budget)
    {
        int eligible = 0, conflicts = 0, noSaving = 0;
        // 预算压力下允许有限质量交换；保留差距，避免同需求区域反复换页。
        foreach (var donor in donors.OrderBy(item => item.Error))
        {
            if (donor.Error >= error * 0.8) break;
            eligible++;
            if (!coverage.TryCoarsen(donor.Tile, _ => true, out var coarse) ||
                !coarse.Leaves.Contains(target)) { conflicts++; continue; }
            var next = coarse.PrepareBalancedRefinement(target);
            if (!next.Leaves.Contains(donor.Tile)) { conflicts++; continue; }
            if (coarse.TransitionPageCount(next) >= required) { noSaving++; continue; }
            var result = Publish(coverage, coarse, donor.Tile, donor.Error, ready);
            return result with { BudgetDiagnostic = $"预算回收 {budget} " +
                $"供体=L{donor.Tile.Level}/{donor.Tile.X}/{donor.Tile.Y} SSE={donor.Error:F2} " +
                $"预计过渡={coarse.TransitionPageCount(next)}" };
        }
        return new(coverage, target, error) { CapacityLimited = true,
            BudgetDiagnostic = $"预算受限 {budget} 供体={donors.Count} " +
                $"低需求={eligible} 拓扑冲突={conflicts} 无节省={noSaving}" };
    }

    private static GeoUnifiedSelection Publish(GeoUnifiedCoverage coverage, GeoUnifiedCoverage candidate,
        GeoUnifiedTile target, double error, Func<GeoUnifiedTile, int, bool> ready)
    {
        if (candidate.Leaves.All(tile => ready(tile, candidate.EdgeMask(tile))))
            return new(candidate, null, 0);
        return new(coverage, target, error) { PendingCoverage = candidate };
    }
}