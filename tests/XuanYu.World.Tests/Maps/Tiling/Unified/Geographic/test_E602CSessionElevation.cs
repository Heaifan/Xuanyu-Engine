using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602CSessionElevation
{
    [Fact]
    public async Task DatasetQuery_ReadsRawHeightOutsideCoarseGrid()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120", (1800, 1800, 777));
        var point = new GeoCoordinate(23.5, 120.5, 0);
        var catalog = new NasademTileCatalog([path]);
        using var session = await GeoUnifiedLodSession.CreateDatasetAsync(
            catalog, new EnuFrame(point), 512);
        Assert.All(session.Coverage.Leaves, tile => Assert.Equal(6, tile.Level));
        var result = session.SampleElevation(point);
        Assert.Equal(GeoSampleStatus.Valid, result.Status);
        Assert.Equal(777d, result.HeightMeters);
        Assert.Equal(MapVerticalDatum.Egm96Orthometric, result.Datum);
        Assert.Equal("g07-viewport", result.Version);
        Assert.Equal(result.HeightMeters, session.SampleImportedHeight(23.5, 120.5));
        var outside = session.SampleElevation(new GeoCoordinate(23.5, 119.99, 0));
        Assert.Equal(GeoSampleStatus.OutsideCoverage, outside.Status);
        Assert.Null(outside.HeightMeters);
        Assert.Null(session.SampleImportedHeight(23.5, 119.99));
        Assert.Equal(0d, session.SampleGridHeight(23.5, 119.99));
        session.Dispose();
        Assert.Throws<ObjectDisposedException>(() => session.SampleElevation(point));
    }
}