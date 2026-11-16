using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02DSelection
{
    private static GeoTileView View(int height = 900) =>
        new(new EcefPosition(3d * Wgs84.SemiMajorAxisMeters, 0, 0),
            new EcefPosition(0, 0, 0), Math.PI / 3d,
            height * 16 / 9, height);

    [Fact]
    public void MaximumBudget_DoesNotDropVisibleRootCoverage()
    {
        var selector = new GeoTileSelector(1_000_000d, 0.5d,
            maxLevel: 8, maxTiles: 2);
        GeoTileId[] result = selector.Select(View()).ToArray();
        Assert.Equal(2, result.Length);
        Assert.Contains(new GeoTileId(0, 0, 0), result);
        Assert.Contains(new GeoTileId(0, 1, 0), result);
    }

    [Fact]
    public void Refinement_RespectsBudgetAndHasNoOverlappingAncestors()
    {
        var selector = new GeoTileSelector(500_000d, 1d,
            maxLevel: 8, maxTiles: 32);
        GeoTileId[] result = selector.Select(View()).ToArray();
        Assert.InRange(result.Length, 3, 32);
        Assert.Equal(result.Length, result.Distinct().Count());
        var unique = result.ToHashSet();
        foreach (GeoTileId tile in result)
        {
            GeoTileId? parent = GeographicTileScheme.Parent(tile);
            while (parent is GeoTileId ancestor)
            {
                Assert.DoesNotContain(ancestor, unique);
                parent = GeographicTileScheme.Parent(ancestor);
            }
        }
    }

    [Fact]
    public void MaximumLevelZero_PreventsRefinement()
    {
        var selector = new GeoTileSelector(1_000_000d, 0.01d,
            maxLevel: 0, maxTiles: 128);
        Assert.All(selector.Select(View()), tile => Assert.Equal(0, tile.Level));
    }

    [Fact]
    public void HigherViewportResolution_RequestsFinerTiles()
    {
        var selector = new GeoTileSelector(10_000d, 2d,
            maxLevel: 6, maxTiles: 128);
        var low = selector.Select(View(500));
        var high = selector.Select(View(2000));
        Assert.True(high.Max(id => id.Level) > low.Max(id => id.Level));
    }

    [Fact]
    public void LargerAllowedScreenError_KeepsCoarserTiles()
    {
        var fine = new GeoTileSelector(30_000d, 2d, maxLevel: 6, maxTiles: 128);
        var coarse = new GeoTileSelector(30_000d, 10d, maxLevel: 6, maxTiles: 128);
        Assert.True(fine.Select(View()).Max(id => id.Level) >
            coarse.Select(View()).Max(id => id.Level));
    }

    [Fact]
    public void Selector_RejectsInvalidBudgetsAndErrorModel()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTileSelector(0, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTileSelector(1000, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTileSelector(1000, 2, maxTiles: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoTileSelector(1000, 2, heightMarginMeters: -1));
    }
}