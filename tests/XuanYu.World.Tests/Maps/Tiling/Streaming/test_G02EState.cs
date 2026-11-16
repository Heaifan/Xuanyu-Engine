using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02EState
{
    [Fact]
    public void State_DistinguishesExactAndParentFallback()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        GeoTileId parent = new(8, 427, 94);
        GeoHeightTile heights = GeoHeightTileBuilder.Build(
            new NasademElevationSource(hgt.Load()), parent, 8);
        GeoTileId child = GeographicTileScheme.Children(parent)[0];
        Assert.Equal(81, heights.ValidSampleCount);

        var exact = new GeoTerrainFrame.Tile(parent, heights);
        var fallback = new GeoTerrainFrame.Tile(child, heights);
        Assert.True(exact.IsExact);
        Assert.False(exact.IsFallback);
        Assert.True(fallback.IsFallback);
        Assert.False(fallback.IsExact);
        Assert.False(exact.HasMissingSamples);
    }

    [Fact]
    public void InvalidBuildBudget_IsRejectedAtPublicEntry()
    {
        var source = new NasademElevationSource();
        var tiles = new GeoTerrainTileSession(source);
        var viewSession = new GeoTerrainViewSession(
            new GeoTileSelector(1000d, 2d, maxLevel: 0), tiles);
        var view = new GeoTileView(
            new EcefPosition(3d * Wgs84.SemiMajorAxisMeters, 0, 0),
            new EcefPosition(0, 0, 0), Math.PI / 3d, 800, 600);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            viewSession.Update(view, 0));
        Assert.Equal(0, tiles.PendingCount);
    }
}