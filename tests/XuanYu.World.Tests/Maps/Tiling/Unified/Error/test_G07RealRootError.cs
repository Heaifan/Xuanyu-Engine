using System.Diagnostics;
using Xunit.Abstractions;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07RealRootError(ITestOutputHelper output)
{
    [G07DataFact]
    public async Task RootErrorAndBoundsUseAllRealFineSamples()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var tile = new GeoUnifiedTile(6, 529, 199);
        using var lease = await pages.RequestAsync(tile);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        var timer = Stopwatch.StartNew();
        double? error = await Task.Run(() => GeoUnifiedError.Measure(lease.Page, source, anchor, 0));
        double errorSeconds = timer.Elapsed.TotalSeconds;
        timer.Restart();
        var bounds = await Task.Run(() => GeoUnifiedBounds.Measure(tile, source, anchor));
        Assert.NotNull(error); Assert.True(error > 0); Assert.NotNull(bounds);
        output.WriteLine($"L6 原始格点误差={error:F3}m，误差耗时={errorSeconds:F2}s，包围体耗时={timer.Elapsed.TotalSeconds:F2}s");
    }
}
