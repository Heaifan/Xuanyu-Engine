using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07Bounds
{
    [G07DataFact]
    public async Task RawSurfaceBoundsProvideConservativeCameraDistance()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var tile = new GeoUnifiedTile(0, 33861, 12736);
        var pages = new GeoUnifiedPageLoader(catalog, loader, new GeoUnifiedPageCache(loader, source));
        using var page = await pages.RequestAsync(tile);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        var bounds = GeoUnifiedBounds.Measure(tile, source, anchor)!.Value;
        var center = new EnuPosition((bounds.Min.East + bounds.Max.East) / 2,
            (bounds.Min.North + bounds.Max.North) / 2, (bounds.Min.Up + bounds.Max.Up) / 2);
        Assert.Equal(0, bounds.DistanceTo(center));
        Assert.Equal(10, bounds.DistanceTo(new EnuPosition(bounds.Max.East + 10, center.North, center.Up)), 6);
        for (int y = 0; y <= 32; y++)
            for (int x = 0; x <= 32; x++)
            {
                var geo = new GeoCoordinate((tile.South + y) / 3600d - 90,
                    (tile.West + x) / 3600d - 180, 0);
                var p = anchor.ToEnu(Wgs84.ToEcef(geo));
                var actual = new EnuPosition(p.East, p.North, p.Up + page.Page.HeightAt(x, y)!.Value);
                Assert.Equal(0, bounds.DistanceTo(actual));
            }
    }
}
