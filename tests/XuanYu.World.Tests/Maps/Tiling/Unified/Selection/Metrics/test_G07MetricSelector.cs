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
