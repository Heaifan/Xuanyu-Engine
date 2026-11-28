using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602CGeographicSamples
{
    [Fact]
    public void Bilinear_UsesRawNodesAndPreservesDatum()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120", (100, 200, 10), (101, 200, 30),
            (100, 201, 50), (101, 201, 70));
        var source = new GeoUnifiedSamples(new NasademTileCatalog([path]), "v1");
        source.Add(NasademHgtFile.Load(path));
        var result = source.SampleElevation(At(100.25, 200.5));
        Assert.Equal(GeoSampleStatus.Valid, result.Status);
        Assert.Equal(35d, result.HeightMeters!.Value, 6);
        Assert.Equal(MapVerticalDatum.Egm96Orthometric, result.Datum);
        Assert.Equal("v1", result.Version);
        Assert.Equal(result, source.SampleElevation(new GeoCoordinate(
            At(100.25, 200.5).LatitudeDegrees, At(100.25, 200.5).LongitudeDegrees, 9000)));
    }

    [Fact]
    public void ExactGrid_ZeroNegativeAndNoDataRemainDistinct()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120", (100, 200, -15), (101, 200, short.MinValue));
        var source = new GeoUnifiedSamples(new NasademTileCatalog([path]), "v1");
        source.Add(NasademHgtFile.Load(path));
        Assert.Equal(-15d, source.SampleElevation(At(100, 200)).HeightMeters);
        Assert.Equal(0d, source.SampleElevation(At(99, 200)).HeightMeters);
        var missing = source.SampleElevation(At(100.5, 200));
        Assert.Equal(GeoSampleStatus.NoData, missing.Status);
        Assert.Null(missing.HeightMeters);
    }

    [Fact]
    public void NotLoadedAndOutsideCoverageNeverReturnZero()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var source = new GeoUnifiedSamples(new NasademTileCatalog([path]), "v1");
        var pending = source.SampleElevation(At(100, 200));
        Assert.Equal(GeoSampleStatus.NotLoaded, pending.Status);
        Assert.Null(pending.HeightMeters);
        source.Add(NasademHgtFile.Load(path));
        var outside = source.SampleElevation(At(-0.5, 200));
        Assert.Equal(GeoSampleStatus.OutsideCoverage, outside.Status);
        Assert.Null(outside.HeightMeters);
        Assert.Equal(0d, source.SampleElevation(At(100, 200)).HeightMeters);
    }

    [Fact]
    public void SharedEdge_UsesPriorityTileAndInterpolatesContinuously()
    {
        using var fixture = new E602CHgtFixture();
        string west = fixture.Create("N23E120", (3599, 200, 100), (3600, 200, 999));
        string east = fixture.Create("N23E121", (0, 200, 200), (1, 200, 300));
        var source = new GeoUnifiedSamples(new NasademTileCatalog([west, east]), "v1");
        source.Add(NasademHgtFile.Load(west));
        Assert.Equal(GeoSampleStatus.NotLoaded, source.SampleElevation(At(3599.5, 200)).Status);
        source.Add(NasademHgtFile.Load(east));
        Assert.Equal(150d, source.SampleElevation(At(3599.5, 200)).HeightMeters!.Value, 6);
        Assert.Equal(200d, source.SampleElevation(At(3600, 200)).HeightMeters);
        Assert.Equal(250d, source.SampleElevation(At(3600.5, 200)).HeightMeters!.Value, 6);
    }

    [Fact]
    public void DateLine_UsesSameWrappedGrid()
    {
        using var fixture = new E602CHgtFixture();
        string west = fixture.Create("N23E179", (3599, 200, 100));
        string east = fixture.Create("N23W180", (0, 200, 200), (1, 200, 300));
        var source = new GeoUnifiedSamples(new NasademTileCatalog([west, east]), "v1");
        source.Add(NasademHgtFile.Load(west)); source.Add(NasademHgtFile.Load(east));
        double lat = 23 + 200d / 3600;
        Assert.Equal(150d, source.SampleElevation(new(lat, 180 - 0.5 / 3600, 0)).HeightMeters!.Value, 6);
        Assert.Equal(source.SampleElevation(new(lat, 180, 0)), source.SampleElevation(new(lat, -180, 0)));
        Assert.Equal(250d, source.SampleElevation(new(lat, -180 + 0.5 / 3600, 0)).HeightMeters!.Value, 6);
    }

    private static GeoCoordinate At(double x, double y) => new(23 + y / 3600, 120 + x / 3600, 0);
}