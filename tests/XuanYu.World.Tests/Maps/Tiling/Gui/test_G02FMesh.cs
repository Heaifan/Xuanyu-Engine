using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02FMesh
{
    [Fact]
    public void RealHgtSample_CreatesMeterScaleRenderMesh()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        hgt.SetHeight(1800, 1800, 800);
        NasademTile source = hgt.Load();
        GeoTileId id = Assert.Single(GeographicTileScheme.Cover(
            new GeoTileBounds(23.5, 120.5, 23.51, 120.51), 13));
        GeoHeightTile tile = GeoHeightTileBuilder.Build(
            new NasademElevationSource(source), id, 16);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 120.5, 0));
        var mesh = GeoTerrainMeshBuilder.Build(tile, anchor);

        Assert.NotNull(mesh);
        Assert.True(mesh.TriangleCount > 0);
        Assert.Equal(0, mesh.Vertices.Length % 21);
        Assert.All(mesh.Vertices.ToArray(), value => Assert.True(float.IsFinite(value)));
    }

    [Fact]
    public void MissingData_DoesNotCreateFakeSeaLevelTriangles()
    {
        var tile = GeoHeightTileBuilder.Build(
            new NasademElevationSource(), new GeoTileId(8, 427, 94), 16);
        Assert.Null(GeoTerrainMeshBuilder.Build(tile,
            new EnuFrame(new GeoCoordinate(23.5, 120.5, 0))));
    }
}