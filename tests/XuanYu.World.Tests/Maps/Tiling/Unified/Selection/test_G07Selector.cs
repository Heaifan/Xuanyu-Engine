using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07Selector
{
    [Fact]
    public void DoesNotReadErrorForParentOutsideCommittedCoverage()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var coverage = new GeoUnifiedCoverage(root);
        var selected = new GeoUnifiedSelector().Step(coverage,
            tile => tile == root ? 0 : throw new InvalidOperationException("无父级误差元数据"), _ => true);
        Assert.Same(coverage, selected.Coverage);
    }

    [Fact]
    public void SelectsHighestErrorAndKeepsCoverageWhenResourcesAreMissing()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var initial = new GeoUnifiedCoverage(root);
        var selector = new GeoUnifiedSelector();
        var waiting = selector.Step(initial, _ => 10, _ => false);
        Assert.Same(initial, waiting.Coverage);
        Assert.Equal(root, waiting.BlockedTile);
        Assert.Equal(10, waiting.BlockedPixels);
        var refined = selector.Step(initial, _ => 10, _ => true);
        Assert.Equal(4, refined.Coverage.Leaves.Count);
        var target = root.Child(1, 1);
        var next = selector.Step(refined.Coverage, tile => tile == target ? 20 : 3, _ => true);
        Assert.DoesNotContain(target, next.Coverage.Leaves);
        Assert.Equal(7, next.Coverage.Leaves.Count);
    }
}
