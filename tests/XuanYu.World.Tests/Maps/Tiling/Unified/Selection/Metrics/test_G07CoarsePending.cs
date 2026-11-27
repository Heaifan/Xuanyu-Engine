using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07CoarsePending
{
    [Fact]
    public void CoarseCandidateRemainsPendingUntilReady()
    {
        var root = new GeoUnifiedTile(1, 16931, 6368);
        var initial = new GeoUnifiedCoverage(root);
        Assert.True(initial.TryRefine(root, _ => true, out var four));
        var selector = new GeoUnifiedSelector();
        var pending = selector.StepWithMetrics(four, (_, _) => 1, (tile, _) => tile != root);
        Assert.Same(four, pending.Coverage);
        Assert.Equal(root, Assert.Single(pending.PendingCoverage!.Leaves));
        var committed = selector.StepWithMetrics(four, (_, _) => 1, (_, _) => true);
        Assert.Equal(root, Assert.Single(committed.Coverage.Leaves));
        Assert.Null(committed.PendingCoverage);
    }
}