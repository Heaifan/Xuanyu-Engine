using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07VisibilitySelection
{
    [Fact]
    public void InvisibleLeafDoesNotConsumeRefinementOpportunity()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var initial = new GeoUnifiedCoverage(root);
        Assert.True(initial.TryRefine(root, _ => true, out var coverage));
        var hidden = root.Child(0, 0);
        var visible = root.Child(1, 0);
        var selector = new GeoUnifiedSelector();
        var selected = selector.Step(coverage, tile => tile == hidden ? 100 : tile == visible ? 10 : 2,
            _ => true, tile => tile != hidden);
        Assert.Contains(hidden, selected.Coverage.Leaves);
        Assert.DoesNotContain(visible, selected.Coverage.Leaves);
    }
}
