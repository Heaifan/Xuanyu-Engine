using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E01AGeometry
{
    [Fact]
    public async Task PageAndGroundPlacementAgreeEvenAwayFromAnchor()
    {
        using var fixture = new E602CHgtFixture();
        var tile = new GeoUnifiedTile(0, 33806, 12768);
        int x = tile.West + 16, y = tile.South + 16;
        string path = fixture.Create("N23E120", (x - 1080000, y - 406800, 777));
        var catalog = new NasademTileCatalog([path]);
        var geoid = LoadGeoid();
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var samples = new GeoUnifiedSamples(catalog, "e602e01a", loader, geoid);
        var cache = new GeoUnifiedPageCache(loader, samples);
        var anchor = new EnuFrame(new GeoCoordinate(22.5, 119.5, 0));
        using var cpu = await GeoUnifiedLodSession.CreateAsync(tile,
            new GeoUnifiedPageLoader(catalog, loader, cache), cache, loader, samples, anchor);
        using var lease = cpu.AcquirePage(tile, 0)!;
        var page = lease.Page;
        Assert.Equal<short?>(777, page.HeightAt(16, 16));
        Assert.Equal(1089, page.GeoidCentimeters.Length);
        var world = new WorldScene();
        var id = world.Create("A", SpatialPosition.FromGeo(new(23.5, 120.5, 0))).Id;
        double lat = y / 3600d - 90d, lon = x / 3600d - 180d;
        var placement = new GeoTerrainPlacement(cpu, geoid);
        Assert.Equal(777d, placement.PlaceOnGround(world, id, lat, lon).HeightMeters);
        var expected = world.GetSpatialPosition(id).ToEnu(anchor);
        var actual = page.PositionAt(anchor, 16, 16);
        double error = Math.Sqrt(Math.Pow(expected.East - actual.East, 2) +
            Math.Pow(expected.North - actual.North, 2) + Math.Pow(expected.Up - actual.Up, 2));
        Assert.InRange(error, 0d, 0.0051d);
        var legacy = GeoUnifiedPosition.At(anchor, x, y, 777);
        Assert.True(Math.Abs(legacy.East - actual.East) > 5d);
    }

    [Fact]
    public async Task CorrectedDatasetPickingRetainsOrthometricElevation()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var point = new GeoCoordinate(23.5, 120.5, 0);
        var geoid = LoadGeoid();
        using var cpu = await GeoUnifiedLodSession.CreateDatasetAsync(
            new NasademTileCatalog([path]), new EnuFrame(point), 512, geoid: geoid);
        Assert.True(cpu.TryPickSurfaceHit(new Vector3(0, 0, 1000), -Vector3.UnitZ, out var hit));
        Assert.InRange((double)hit.Position.Z, 20d, 22d);
        Assert.Equal(0f, hit.Elevation);
        Assert.Equal(0d, cpu.SampleElevation(point).HeightMeters);
    }

    private static Egm96GeoidGrid LoadGeoid() => Egm96GeoidGrid.Load(
        Environment.GetEnvironmentVariable("XYE_EGM96_GRID") ??
        throw new InvalidOperationException("请设置 XYE_EGM96_GRID。"));
}