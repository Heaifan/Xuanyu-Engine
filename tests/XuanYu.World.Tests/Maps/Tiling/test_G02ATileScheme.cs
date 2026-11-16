using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02ATileScheme
{
    [Fact]
    public void LevelZero_CoversTwoEarthHemispheres()
    {
        Assert.Equal(2, GeographicTileScheme.TileCountX(0));
        Assert.Equal(1, GeographicTileScheme.TileCountY(0));
        Assert.Equal(new GeoTileBounds(-90, -180, 90, 0),
            GeographicTileScheme.Bounds(new GeoTileId(0, 0, 0)));
        Assert.Equal(new GeoTileBounds(-90, 0, 90, 180),
            GeographicTileScheme.Bounds(new GeoTileId(0, 1, 0)));
    }

    [Fact]
    public void Find_UsesNormalizedLongitudeAndNorthOrigin()
    {
        Assert.Equal(new GeoTileId(0, 0, 0),
            GeographicTileScheme.Find(new GeoCoordinate(0, -180, 0), 0));
        Assert.Equal(new GeoTileId(0, 0, 0),
            GeographicTileScheme.Find(new GeoCoordinate(0, 180, 0), 0));
        Assert.Equal(new GeoTileId(1, 2, 0),
            GeographicTileScheme.Find(new GeoCoordinate(90, 0, 0), 1));
        Assert.Equal(new GeoTileId(1, 2, 1),
            GeographicTileScheme.Find(new GeoCoordinate(-90, 0, 0), 1));
    }

    [Fact]
    public void ParentAndChildren_KeepQuadTreeTopology()
    {
        var root = new GeoTileId(0, 1, 0);
        GeoTileId[] children = GeographicTileScheme.Children(root);
        GeoTileId[] expected = [new GeoTileId(1, 2, 0), new GeoTileId(1, 3, 0),
            new GeoTileId(1, 2, 1), new GeoTileId(1, 3, 1)];
        Assert.Equal(expected, children);
        Assert.Null(GeographicTileScheme.Parent(root));
        foreach (GeoTileId child in children)
            Assert.Equal(root, GeographicTileScheme.Parent(child));
    }

    [Fact]
    public void Neighbors_WrapLongitudeButStopAtPoles()
    {
        var eastEdge = new GeoTileId(3, 15, 3);
        Assert.Equal(new GeoTileId(3, 0, 3),
            GeographicTileScheme.East(eastEdge));
        Assert.Equal(eastEdge,
            GeographicTileScheme.West(GeographicTileScheme.East(eastEdge)));
        Assert.Null(GeographicTileScheme.North(new GeoTileId(3, 3, 0)));
        Assert.Null(GeographicTileScheme.South(new GeoTileId(3, 3, 7)));
    }

    [Fact]
    public void HighestLevel_DoesNotOverflowTileIndices()
    {
        int level = GeoTileId.MaxLevel;
        int maxX = GeographicTileScheme.TileCountX(level) - 1;
        int maxY = GeographicTileScheme.TileCountY(level) - 1;
        var last = new GeoTileId(level, maxX, maxY);
        Assert.Equal(new GeoTileId(level, 0, maxY),
            GeographicTileScheme.East(last));
        Assert.True(GeographicTileScheme.Bounds(last).EastLongitudeDegrees <= 180);
    }

    [Fact]
    public void InvalidTileIndex_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoTileId(-1, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoTileId(0, 2, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoTileId(1, 0, 2));
        Assert.Throws<InvalidOperationException>(() =>
            GeographicTileScheme.Children(new GeoTileId(GeoTileId.MaxLevel, 0, 0)));
    }
}