using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07BalancedSelection
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefinementWaitsForRequiredCoarseNeighborAndCommitsTogether(bool metrics)
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var initial = new GeoUnifiedCoverage(root);
        Assert.True(initial.TryRefine(root, _ => true, out var four));
        var southwest = root.Child(0, 0);
        var east = root.Child(1, 0);
        Assert.True(four.TryRefine(southwest, _ => true, out var seven));
        var target = southwest.Child(1, 0);
        var required = east.Child(0, 0);
        Assert.False(seven.TryRefine(target, _ => true, out _));
        var selector = new GeoUnifiedSelector();
        double Error(GeoUnifiedTile tile) => tile == target ? 20 : 2;
        GeoUnifiedSelection Select(Func<GeoUnifiedTile, int, bool> ready) => metrics
            ? selector.StepWithMetrics(seven, (tile, _) => Error(tile), ready)
            : selector.StepWithEdges(seven, Error, ready);
        bool requested = false;
        var waiting = Select((tile, _) =>
        {
            if (tile != required) return true;
            requested = true;
            return false;
        });
        Assert.True(requested);
        Assert.Same(seven, waiting.Coverage);
        Assert.Equal(target, waiting.BlockedTile);
        var committed = Select((_, _) => true).Coverage;
        Assert.Equal(13, committed.Leaves.Count);
        Assert.DoesNotContain(target, committed.Leaves);
        Assert.DoesNotContain(east, committed.Leaves);
        Assert.Contains(required, committed.Leaves);
        Assert.Contains(root.Child(0, 1), committed.Leaves);
        Assert.Contains(root.Child(1, 1), committed.Leaves);
        Assert.False(committed.TryCoarsen(east, _ => true, out _));
    }
}