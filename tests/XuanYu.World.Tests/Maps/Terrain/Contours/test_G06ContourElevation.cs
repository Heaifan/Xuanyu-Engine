using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06ContourElevation
{
    [Fact]
    public void RawSampleSpike_ProducesClosedContourWithoutRenderMesh()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(1800, 1800, 1000);
        NasademTile tile = hgt.Load();
        var source = new NasademElevationSource(tile);
        var center = new GeoCoordinate(24.5d, 121.5d, 0d);
        var id = GeographicTileScheme.Find(center, 13);
        var bounds = new GeoTileBounds(24d, 121d, 25d, 122d);
        var task = new GeoContourElevationTask(id, bounds, source,
            new EnuFrame(center), new GeoContourStyle(100, 0.3f, 2f, 1f), 1);
        while (!task.Advance(8)) { }
        var mesh = task.Finish();
        Assert.NotNull(mesh);
        Assert.True(mesh.VertexCount >= 24);
        Assert.All(mesh.Vertices.ToArray(), value => Assert.True(float.IsFinite(value)));
    }

    [Fact]
    public void ConstantRawHeight_DoesNotFabricateContour()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var source = new NasademElevationSource(hgt.Load());
        var center = new GeoCoordinate(24.5d, 121.5d, 0d);
        var task = new GeoContourElevationTask(
            GeographicTileScheme.Find(center, 13),
            new GeoTileBounds(24d, 121d, 25d, 122d), source,
            new EnuFrame(center), new GeoContourStyle(100, 0.3f, 2f, 1f), 1);
        while (!task.Advance(8)) { }
        Assert.Null(task.Finish());
    }
}