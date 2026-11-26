using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07CoverageCommit
{
    [Fact]
    public void ParentReplacementIsAtomicAndRejectsUnbalancedChildren()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var initial = new GeoUnifiedCoverage(root);
        Assert.False(initial.TryRefine(root, _ => false, out var waiting));
        Assert.Same(initial, waiting);
        Assert.True(initial.TryRefine(root, _ => true, out var four));
        Assert.Equal(4, four.Leaves.Count);
        var southwest = root.Child(0, 0);
        var eastNeighbor = root.Child(1, 0);
        Assert.False(four.TryRefine(southwest, tile => tile != eastNeighbor, out var neighborWaiting));
        Assert.Same(four, neighborWaiting);
        Assert.True(four.TryRefine(southwest, _ => true, out var seven));
        Assert.Equal(7, seven.Leaves.Count);
        var touchingEast = southwest.Child(1, 0);
        Assert.False(seven.TryRefine(touchingEast, _ => true, out var rejected));
        Assert.Same(seven, rejected);
        Assert.True(seven.TryCoarsen(southwest, _ => true, out var restored));
        Assert.Equal(four.Leaves.OrderBy(t => t.X).ThenBy(t => t.Y),
            restored.Leaves.OrderBy(t => t.X).ThenBy(t => t.Y));
    }
}
