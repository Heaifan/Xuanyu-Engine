using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07CapacitySelection
{
    [Fact]
    public void FullBudgetTradesLowerDemandBeforeAtomicRefinement()
    {
        var (coverage, target, donor) = NearCapacity();
        var selector = new GeoUnifiedSelector();
        double? Error(GeoUnifiedTile tile, int _) => tile == target ? 100d : tile == donor ? 10d : 120d;
        double? Demand(GeoUnifiedTile tile, int mask) => tile.Level == 1 &&
            coverage.Leaves.Contains(tile) && tile != target ? 2d : Error(tile, mask);
        var waiting = selector.StepWithMetrics(coverage, Demand, (tile, _) => coverage.Leaves.Contains(tile),
            capacity: 512);
        Assert.Same(coverage, waiting.Coverage);
        Assert.NotNull(waiting.PendingCoverage);
        Assert.Equal(donor, waiting.BlockedTile);
        Assert.Equal(508, waiting.PendingCoverage!.Leaves.Count);
        Assert.Equal(512, coverage.Leaves.Union(waiting.PendingCoverage.Leaves).Count());
        var freed = selector.StepWithMetrics(coverage, Demand, (_, _) => true, capacity: 512);
        Assert.Equal(508, freed.Coverage.Leaves.Count);
        Assert.Contains(donor, freed.Coverage.Leaves);
        var refined = selector.StepWithMetrics(freed.Coverage, Demand, (_, _) => true, capacity: 512);
        Assert.Equal(511, refined.Coverage.Leaves.Count);
        Assert.DoesNotContain(target, refined.Coverage.Leaves);
        Assert.Contains(donor, refined.Coverage.Leaves);
        Assert.Equal(512, freed.Coverage.Leaves.Union(refined.Coverage.Leaves).Count());
        var stable = selector.StepWithMetrics(refined.Coverage,
            (tile, _) => tile == donor ? 10d : tile == target ? 100d : 80d,
            (_, _) => true, capacity: 512, visible: t => t == donor);
        Assert.Same(refined.Coverage, stable.Coverage);
        Assert.True(stable.CapacityLimited);
        Assert.Null(stable.PendingCoverage);
    }

    [Fact]
    public void NoUsefulDonorDoesNotRequestImpossibleCandidate()
    {
        var (coverage, target, _) = NearCapacity();
        var selection = new GeoUnifiedSelector().StepWithMetrics(coverage,
            (tile, _) => tile == target ? 100d : 80d,
            (_, _) => throw new InvalidOperationException("超容量候选不能触发就绪请求。"),
            visible: t => t == target, capacity: 512);
        Assert.Same(coverage, selection.Coverage);
        Assert.True(selection.CapacityLimited);
        Assert.Null(selection.PendingCoverage);
        Assert.Equal(target, selection.BlockedTile);
    }

    [Fact]
    public void ReserveTradesAtSteadyLimitWithoutImmediateReverse()
    {
        var (coverage, target, donor) = NearCapacity(84);
        var selector = new GeoUnifiedSelector();
        double? Error(GeoUnifiedTile tile, int _) => tile == target ? 100d : tile == donor ? 60d :
            tile.Level == 1 && coverage.Leaves.Contains(tile) ? 2d : 120d;
        var waiting = selector.StepWithMetrics(coverage, Error, (tile, _) => coverage.Leaves.Contains(tile),
            capacity: 512, transitionReserve: 4);
        Assert.Same(coverage, waiting.Coverage);
        Assert.Equal(donor, waiting.BlockedTile);
        Assert.Equal(505, waiting.PendingCoverage!.Leaves.Count);
        Assert.Contains("预算回收", waiting.BudgetDiagnostic!);
        var freed = selector.StepWithMetrics(coverage, Error, (_, _) => true, capacity: 512, transitionReserve: 4);
        Assert.Equal(505, freed.Coverage.Leaves.Count);
        var refined = selector.StepWithMetrics(freed.Coverage, Error, (_, _) => true, capacity: 512, transitionReserve: 4);
        Assert.Equal(508, refined.Coverage.Leaves.Count);
        Assert.DoesNotContain(target, refined.Coverage.Leaves);
        Assert.Contains(donor, refined.Coverage.Leaves);
        var stable = selector.StepWithMetrics(refined.Coverage, Error, (_, _) => true,
            t => t == donor, capacity: 512, transitionReserve: 4);
        Assert.Same(refined.Coverage, stable.Coverage);
        Assert.True(stable.CapacityLimited);
        Assert.Null(stable.PendingCoverage);
        Assert.Contains("预算受限", stable.BudgetDiagnostic!);
    }

    private static (GeoUnifiedCoverage Coverage, GeoUnifiedTile Target, GeoUnifiedTile Donor) NearCapacity(int splitCount = 85)
    {
        var coverage = new GeoUnifiedCoverage(new GeoUnifiedTile(5, 1058, 398));
        for (int level = 5; level > 1; level--)
            foreach (var tile in coverage.Leaves.Where(t => t.Level == level).ToArray())
            {
                Assert.True(coverage.TryRefine(tile, _ => true, out var refined));
                coverage = refined;
            }
        var parents = coverage.Leaves.OrderBy(t => t.Y).ThenBy(t => t.X).ToArray();
        foreach (var parent in parents.Take(splitCount))
        {
            Assert.True(coverage.TryRefine(parent, _ => true, out var refined));
            coverage = refined;
        }
        Assert.Equal(256 + 3 * splitCount, coverage.Leaves.Count);
        return (coverage, parents[^1], parents[0]);
    }
}