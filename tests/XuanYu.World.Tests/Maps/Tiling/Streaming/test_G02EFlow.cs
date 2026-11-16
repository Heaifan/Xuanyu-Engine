using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02EFlow
{
    private static GeoTileView EarthView() => new(
        new EcefPosition(3d * Wgs84.SemiMajorAxisMeters, 0, 0),
        new EcefPosition(0, 0, 0), Math.PI / 3d, 800, 600);

    private static GeoTileSelector RootSelector() =>
        new(10_000d, 10d, maxLevel: 0, maxTiles: 2);

    [Fact]
    public void Frames_RespectBudgetAndReturnActualNasademHeight()
    {
        using var hgt = new G02BHgtFixture("N00E000");
        hgt.SetHeight(0, 0, 321);
        var tiles = new GeoTerrainTileSession(
            new NasademElevationSource(hgt.Load()), 2, 4);
        var viewSession = new GeoTerrainViewSession(RootSelector(), tiles);

        GeoTerrainFrame first = viewSession.Update(EarthView(), 1);
        Assert.Equal(2, first.Tiles.Count);
        Assert.Equal(1, first.BuiltCount);
        Assert.Equal(1, first.PendingCount);
        Assert.False(Assert.Single(first.Tiles, state =>
            state.RequestedId == new GeoTileId(0, 1, 0)).HasData);

        GeoTerrainFrame second = viewSession.Update(EarthView(), 1);
        Assert.Equal(1, second.BuiltCount);
        Assert.Equal(0, second.PendingCount);
        GeoTerrainFrame.Tile east = Assert.Single(second.Tiles, state =>
            state.RequestedId == new GeoTileId(0, 1, 0));
        Assert.True(east.IsExact);
        Assert.True(east.HasMissingSamples);
        Assert.Equal(321d, east.AvailableTile!.HeightAt(0, 1)!.Value);
    }

    [Fact]
    public void RepeatedView_DoesNotRebuildCachedMissingTiles()
    {
        var tiles = new GeoTerrainTileSession(new NasademElevationSource(), 2, 4);
        var viewSession = new GeoTerrainViewSession(RootSelector(), tiles);
        GeoTerrainFrame first = viewSession.Update(EarthView(), 2);
        GeoTerrainFrame second = viewSession.Update(EarthView(), 2);

        Assert.Equal(2, first.BuiltCount);
        Assert.Equal(0, second.BuiltCount);
        Assert.Equal(0, second.PendingCount);
        Assert.All(second.Tiles, state => Assert.False(state.HasData));
        Assert.Equal(2, tiles.CachedCount);
    }

    [Fact]
    public void AwayFromEarth_ReturnsNoTilesAndSkipsProcessing()
    {
        var tiles = new GeoTerrainTileSession(new NasademElevationSource());
        var viewSession = new GeoTerrainViewSession(RootSelector(), tiles);
        Assert.Equal(1, viewSession.Update(EarthView(), 1).PendingCount);
        var eye = new EcefPosition(3d * Wgs84.SemiMajorAxisMeters, 0, 0);
        var away = new EcefPosition(4d * Wgs84.SemiMajorAxisMeters, 0, 0);

        GeoTerrainFrame frame = viewSession.Update(
            new GeoTileView(eye, away, Math.PI / 3d, 800, 600), 1);
        Assert.Empty(frame.Tiles);
        Assert.Equal(0, frame.BuiltCount);
        Assert.Equal(1, frame.PendingCount);
    }
}