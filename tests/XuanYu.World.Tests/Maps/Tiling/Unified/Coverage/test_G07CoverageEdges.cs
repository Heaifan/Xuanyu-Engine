using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07CoverageEdges
{
    [Fact]
    public void EdgeMasksComeFromCommittedNeighborsAndChangeAtomically()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var cover = new GeoUnifiedCoverage(root);
        Assert.True(cover.TryRefine(root, _ => true, out var four));
        var southwest = root.Child(0, 0);
        Assert.True(four.TryRefine(southwest, _ => true, out var seven));
        var eastChild = southwest.Child(1, 0);
        var northChild = southwest.Child(0, 1);
        Assert.Equal(8, seven.EdgeMask(eastChild));
        Assert.Equal(2, seven.EdgeMask(northChild));
        Assert.False(four.TryRefine(southwest, child => child != eastChild, out var rejected));
        Assert.Same(four, rejected);
        Assert.Equal(0, four.EdgeMask(southwest));
    }
}
