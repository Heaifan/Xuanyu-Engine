using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07CoverageEdges
{
    [Fact]
    public void AllFourCoarseNeighborDirectionsMatchTheCommittedCover()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var coverage = new GeoUnifiedCoverage(root);
        Assert.True(coverage.TryRefine(root, _ => true, out var four));
        var sw = root.Child(0, 0);
        var ne = root.Child(1, 1);
        Assert.True(four.TryRefine(sw, _ => true, out var seven));
        Assert.Equal(2 | 8, seven.EdgeMask(sw.Child(1, 1)));
        Assert.True(seven.TryRefine(ne, _ => true, out var ten));
        Assert.Equal(1 | 4, ten.EdgeMask(ne.Child(0, 0)));
        Assert.True(ten.TryCoarsen(ne, _ => true, out var restored));
        Assert.Equal(2 | 8, restored.EdgeMask(sw.Child(1, 1)));
    }

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