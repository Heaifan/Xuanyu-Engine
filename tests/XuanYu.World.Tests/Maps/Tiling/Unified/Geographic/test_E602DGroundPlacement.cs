using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602DGroundPlacement
{
    [Fact]
    public async Task GroundPlacement_UpdatesOnlyPositionAndPreservesCloneIsolation()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120", (1800, 1800, 777));
        using var terrain = await Open(path);
        var placement = new GeoTerrainPlacement(terrain, LoadGeoid());
        var world = new WorldScene();
        var id = world.Create("A", SpatialPosition.FromGeo(new(23.25, 120.25, 100))).Id;
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.5f);
        var scale = new Vector3(2, 3, 4);
        var transform = world.Find(id)!.Transform with { Rotation = rotation, Scale = scale };
        world.SetTransform(id, transform);
        var clone = world.Clone();
        var sample = placement.PlaceOnGround(clone, id, 23.5, 120.5, 2);
        Assert.Equal(GeoSampleStatus.Valid, sample.Status);
        Assert.Equal(777d, sample.HeightMeters);
        Assert.Equal(MapVerticalDatum.Egm96Orthometric, sample.Datum);
        Assert.Equal("g07-viewport", sample.Version);
        AssertPosition(clone, id, 23.5, 120.5, 800.2519912719733);
        Assert.Equal(rotation, clone.Find(id)!.Transform.Rotation);
        Assert.Equal(scale, clone.Find(id)!.Transform.Scale);
        Assert.Equal(transform, world.Find(id)!.Transform);
    }

    [Fact]
    public async Task ZeroAndNegativeHeights_AreValidGround()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120", (1800, 1800, -15));
        using var terrain = await Open(path);
        var geoid = LoadGeoid();
        var placement = new GeoTerrainPlacement(terrain, geoid);
        var world = new WorldScene();
        var id = world.Create("A", SpatialPosition.FromGeo(new(23.5, 120.5, 100))).Id;
        Assert.Equal(-15d, placement.PlaceOnGround(world, id, 23.5, 120.5).HeightMeters);
        AssertPosition(world, id, 23.5, 120.5, 6.251991271973314);
        var target = new GeoCoordinate(23.75, 120.75, 0);
        Assert.Equal(0d, placement.PlaceOnGround(world, id, 23.75, 120.75).HeightMeters);
        AssertPosition(world, id, 23.75, 120.75, geoid.UndulationMeters(target));
    }

    [Fact]
    public async Task FailedQueriesAndInvalidInput_LeaveEntityUnchanged()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120", (1800, 1800, short.MinValue));
        using var terrain = await Open(path);
        var placement = new GeoTerrainPlacement(terrain, LoadGeoid());
        var world = new WorldScene();
        var entity = world.Create("A", SpatialPosition.FromGeo(new(23.25, 120.25, 100)));
        var missing = placement.PlaceOnGround(world, entity.Id, 23.5, 120.5);
        Assert.Equal(GeoSampleStatus.NoData, missing.Status);
        Assert.Null(missing.HeightMeters);
        var outside = placement.PlaceOnGround(world, entity.Id, 23.5, 119.5);
        Assert.Equal(GeoSampleStatus.OutsideCoverage, outside.Status);
        Assert.Null(outside.HeightMeters);
        Assert.Throws<ArgumentOutOfRangeException>(() => placement.PlaceOnGround(world, entity.Id, 23.5, 120.5, -1));
        var local = world.Create("Local");
        Assert.Throws<InvalidOperationException>(() => placement.PlaceOnGround(world, local.Id, 23.5, 120.5));
        Assert.Equal(entity, world.Find(entity.Id));
        Assert.Equal(local, world.Find(local.Id));
    }

    private static Task<GeoUnifiedLodSession> Open(string path) =>
        GeoUnifiedLodSession.CreateDatasetAsync(new NasademTileCatalog([path]),
            new EnuFrame(new GeoCoordinate(23.5, 120.5, 0)), 512);

    private static Egm96GeoidGrid LoadGeoid() => Egm96GeoidGrid.Load(
        Environment.GetEnvironmentVariable("XYE_EGM96_GRID") ??
        throw new InvalidOperationException("请设置 XYE_EGM96_GRID。"));

    private static void AssertPosition(WorldScene world, Guid id, double lat, double lon, double height)
    {
        var geo = world.GetSpatialPosition(id).ToGeo();
        Assert.InRange(Math.Abs(geo.LatitudeDegrees - lat), 0, 1e-9);
        Assert.InRange(Math.Abs(geo.LongitudeDegrees - lon), 0, 1e-9);
        Assert.InRange(Math.Abs(geo.HeightMeters - height), 0, 1e-5);
    }
}