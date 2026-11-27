using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07MetricSelector
{
    [Fact]
    public void MissingMetricPreservesCoverageAndIsReported()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var coverage = new GeoUnifiedCoverage(root);
        var result = new GeoUnifiedSelector().StepWithMetrics(coverage, (_, _) => null, (_, _) => true);
        Assert.Same(coverage, result.Coverage);
        Assert.Equal(root, result.BlockedTile);
        Assert.True(double.IsNaN(result.BlockedPixels));
    }

    [Fact]
    public void HiddenHighestErrorDoesNotRefineAndReadyCoverageStaysAtomic()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var coverage = new GeoUnifiedCoverage(root);
        Assert.True(coverage.TryRefine(root, _ => true, out var four));
        var hidden = root.Child(0, 0);
        var visible = root.Child(1, 0);
        var selector = new GeoUnifiedSelector();
        double? Error(GeoUnifiedTile tile, int _) => tile == hidden ? 100 : tile == visible ? 10 : 2;
        var waiting = selector.StepWithMetrics(four, Error, (_, _) => false, t => t != hidden);
        Assert.Same(four, waiting.Coverage);
        Assert.Equal(visible, waiting.BlockedTile);
        var committed = selector.StepWithMetrics(four, Error, (_, _) => true, t => t != hidden);
        Assert.Contains(hidden, committed.Coverage.Leaves);
        Assert.DoesNotContain(visible, committed.Coverage.Leaves);
    }

    [Fact]
    public void CoarsenUsesTheCandidateEdgeMaskMetric()
    {
        var root = new GeoUnifiedTile(6, 529, 199);
        var coverage = new GeoUnifiedCoverage(root);
        Assert.True(coverage.TryRefine(root, _ => true, out var four));
        var parent = root.Child(0, 0);
        Assert.True(four.TryRefine(parent, _ => true, out var seven));
        var result = new GeoUnifiedSelector().StepWithMetrics(seven,
            (tile, mask) => tile == parent && mask == 0 ? 1 : 3, (_, _) => true);
        Assert.Contains(parent, result.Coverage.Leaves);
        Assert.Equal(4, result.Coverage.Leaves.Count);
    }
}