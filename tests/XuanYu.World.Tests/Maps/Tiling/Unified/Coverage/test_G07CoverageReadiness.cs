using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07CoverageReadiness
{
    [Fact]
    public void SelectorDoesNotCommitUntilNewEdgeResourcesAreReady()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var initial = new GeoUnifiedCoverage(root);
        Assert.True(initial.TryRefine(root, _ => true, out var four));
        var target = root.Child(0, 0);
        var selector = new GeoUnifiedSelector();
        var waiting = selector.StepWithEdges(four, tile => tile == target ? 10 : 2,
            (_, mask) => mask == 0);
        Assert.Same(four, waiting.Coverage);
        Assert.Equal(target, waiting.BlockedTile);
        var ready = selector.StepWithEdges(four, tile => tile == target ? 10 : 2,
            (_, _) => true);
        Assert.Equal(7, ready.Coverage.Leaves.Count);
    }

    [Fact]
    public void RefinementWaitsForCandidateEdgeMaskResources()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var initial = new GeoUnifiedCoverage(root);
        Assert.True(initial.TryRefineWithEdges(root, (_, _) => true, out var four));
        var child = root.Child(0, 0);
        Assert.False(four.TryRefineWithEdges(child, (_, mask) => mask == 0, out var waiting));
        Assert.Same(four, waiting);
        Assert.True(four.TryRefineWithEdges(child, (_, _) => true, out var seven));
        Assert.Equal(8, seven.EdgeMask(child.Child(1, 0)));
    }
}
