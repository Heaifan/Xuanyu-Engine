using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06ContourMesh
{
    [Fact]
    public void NativeHeightSpike_GeneratesContourTrianglesWithoutChangingTerrain()
    {
        using var file = new G02BHgtFixture("N24E121");
        file.SetHeight(1801, 1801, 500);
        NasademTile tile = file.Load();
        var source = new NasademElevationSource(tile);
        var anchor = new EnuFrame(new GeoCoordinate(24.5d, 121.5d, 0d));
        GeoTileId id = GeographicTileScheme.Find(
            new GeoCoordinate(24.5003d, 121.5003d, 0d), 13);
        var area = new GeoTileBounds(24d, 121d, 25d, 122d);
        RenderMesh terrain = GeoTerrainNativeMeshBuilder.Build(
            tile, id, anchor, source, area, default)!;
        float[] unchanged = terrain.Vertices.ToArray();
        RenderMesh? contours = ContourFrom(terrain, anchor, 100);
        Assert.NotNull(contours);
        Assert.True(contours.TriangleCount > 0);
        Assert.Equal(unchanged, terrain.Vertices.ToArray());
        Assert.All(contours.Vertices.ToArray(), v => Assert.True(float.IsFinite(v)));
    }

    [Fact]
    public void FlatTile_DoesNotGenerateFalseZeroContour()
    {
        using var file = new G02BHgtFixture("N24E121");
        var source = new NasademElevationSource(file.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24.5d, 121.5d, 0d));
        GeoTileId id = GeographicTileScheme.Find(
            new GeoCoordinate(24.5d, 121.5d, 0d), 9);
        RenderMesh terrain = GeoTerrainMeshBuilder.Build(
            GeoHeightTileBuilder.Build(source, id, 16), anchor)!;
        Assert.Null(ContourFrom(terrain, anchor, 100));
    }

    private static RenderMesh? ContourFrom(RenderMesh terrain,
        EnuFrame anchor, int interval) => GeoContourMesh.Build(
            terrain, anchor, new GeoContourStyle(interval, 0.3f, 1f, 0.3f));
}