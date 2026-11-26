using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07LodMetric
{
    [G07DataFact]
    public async Task MetricBindsActualPageMaskAndCameraDistance()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 100_000_000);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var pageLoader = new GeoUnifiedPageLoader(catalog, loader, new GeoUnifiedPageCache(loader, source));
        var tile = new GeoUnifiedTile(0, 33861, 12736);
        using var lease = await pageLoader.RequestAsync(tile);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        var metric = GeoUnifiedLodMetric.Measure(lease.Page, source, anchor, 8)!.Value;
        Assert.Equal(tile, metric.Tile);
        Assert.Equal(8, metric.EdgeMask);
        Assert.True(metric.ErrorMeters > 0);
        var b = metric.Bounds;
        var inside = new EnuPosition((b.Min.East + b.Max.East) / 2,
            (b.Min.North + b.Max.North) / 2, (b.Min.Up + b.Max.Up) / 2);
        Assert.True(double.IsPositiveInfinity(metric.ScreenError(inside, 1080, Math.PI / 3)));
    }
}
