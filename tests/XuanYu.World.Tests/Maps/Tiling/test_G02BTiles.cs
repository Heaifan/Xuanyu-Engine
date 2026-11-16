using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02BTiles
{
    [Fact]
    public void HeightTile_KeepsGeographicBoundsAndEgm96Datum()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var source = new NasademElevationSource(hgt.Load());
        var id = new GeoTileId(8, 427, 94);
        GeoHeightTile tile = GeoHeightTileBuilder.Build(source, id, 2);

        Assert.Equal(id, tile.Id);
        Assert.Equal(GeographicTileScheme.Bounds(id), tile.Bounds);
        Assert.Equal(MapVerticalDatum.Egm96Orthometric, tile.VerticalDatum);
        Assert.Equal(3, tile.SamplesPerSide);
        Assert.Equal(9, tile.ValidSampleCount);
        Assert.Equal(0d, tile.HeightAt(1, 1)!.Value);
    }

    [Fact]
    public void AdjacentTiles_UseIdenticalSharedEdgeSamples()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        hgt.SetHeight(3375, 1996, 277);
        hgt.SetHeight(3375, 1997, 277);
        var source = new NasademElevationSource(hgt.Load());
        GeoHeightTile west = GeoHeightTileBuilder.Build(
            source, new GeoTileId(8, 427, 94), 8);
        GeoHeightTile east = GeoHeightTileBuilder.Build(
            source, new GeoTileId(8, 428, 94), 8);

        for (int row = 0; row < 9; row++)
            Assert.Equal(west.HeightAt(8, row), east.HeightAt(0, row));
        Assert.Equal(277d, west.HeightAt(8, 4)!.Value);
        Assert.Equal(277d, east.HeightAt(0, 4)!.Value);
    }

    [Fact]
    public void DegreeLineTile_UsesBothLoadedNasademSources()
    {
        using var west = new G02BHgtFixture("N23E120");
        using var east = new G02BHgtFixture("N23E121");
        west.SetHeight(3375, 1996, 123);
        west.SetHeight(3375, 1997, 123);
        foreach (int x in new[] { 2306, 2307 })
            foreach (int y in new[] { 1996, 1997 })
                east.SetHeight(x, y, 432);
        var source = new NasademElevationSource(west.Load(), east.Load());
        GeoHeightTile tile = GeoHeightTileBuilder.Build(source,
            new GeoTileId(8, 428, 94), 8);

        Assert.Equal(123d, tile.HeightAt(0, 4)!.Value);
        Assert.Equal(432d, tile.HeightAt(8, 4)!.Value);
        Assert.Equal(81, tile.ValidSampleCount);
    }

    [Fact]
    public void EmptyGeographicArea_StaysMissing()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var source = new NasademElevationSource(hgt.Load());
        GeoHeightTile tile = GeoHeightTileBuilder.Build(source,
            new GeoTileId(8, 0, 0), 4);

        Assert.Equal(0, tile.ValidSampleCount);
        Assert.Null(tile.HeightAt(2, 2));
    }

    [Fact]
    public void MissingSourceSample_PropagatesIntoHeightTile()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        foreach (int x in new[] { 2109, 2110 })
            foreach (int y in new[] { 1996, 1997 })
                hgt.SetHeight(x, y, short.MinValue);
        var source = new NasademElevationSource(hgt.Load());
        GeoHeightTile tile = GeoHeightTileBuilder.Build(source,
            new GeoTileId(8, 427, 94), 8);

        Assert.Null(tile.HeightAt(4, 4));
        Assert.True(tile.ValidSampleCount < 81);
    }
}