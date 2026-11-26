using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07LodSession
{
    [G07DataFact]
    public async Task NonblockingSelectionWaitsForCpuAndGpuReadiness()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var root = new GeoUnifiedTile(6, 529, 199);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        using var session = await GeoUnifiedLodSession.CreateAsync(root, pages, cache, loader, source, anchor);
        var eye = new EnuPosition(0, 0, 10_000);
        var initial = session.Coverage;
        var blocked = session.Step(eye, 1080, Math.PI / 3, (_, _) => false);
        Assert.Same(initial, blocked.Coverage);
        GeoUnifiedSelection result = default;
        for (int i = 0; i < 1500; i++)
        {
            result = session.Step(eye, 1080, Math.PI / 3, (_, _) => true);
            if (result.Coverage.Leaves.Count == 4) break;
            await Task.Delay(10);
        }
        Assert.Equal(4, result.Coverage.Leaves.Count);
        foreach (var tile in result.Coverage.Leaves)
        {
            using var page = session.AcquirePage(tile, result.Coverage.EdgeMask(tile));
            Assert.NotNull(page);
            Assert.Equal(GeoSampleStatus.Valid, page!.Page.StatusAt(0, 0));
        }
    }
}
