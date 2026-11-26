using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07Error
{
    [G07DataFact]
    public async Task FoldedEdgeErrorBoundsRealMidpointDeviation()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var tile = new GeoUnifiedTile(0, 33861, 12736);
        var pages = new GeoUnifiedPageLoader(catalog, loader, new GeoUnifiedPageCache(loader, source));
        using var page = await pages.RequestAsync(tile);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        double plain = GeoUnifiedError.Measure(page.Page, source, anchor, 0)!.Value;
        double folded = GeoUnifiedError.Measure(page.Page, source, anchor, 8)!.Value;
        Assert.InRange(plain, 0, 1e-7);
        double maximum = 0;
        for (int y = 1; y < 32; y += 2)
        {
            var a = Position(anchor, tile.West + 32, tile.South + y - 1, page.Page.HeightAt(32, y - 1)!.Value);
            var b = Position(anchor, tile.West + 32, tile.South + y + 1, page.Page.HeightAt(32, y + 1)!.Value);
            var m = Position(anchor, tile.West + 32, tile.South + y, page.Page.HeightAt(32, y)!.Value);
            double dx = m.East - (a.East + b.East) / 2;
            double dy = m.North - (a.North + b.North) / 2;
            double dz = m.Up - (a.Up + b.Up) / 2;
            maximum = Math.Max(maximum, Math.Sqrt(dx * dx + dy * dy + dz * dz));
        }
        Assert.True(maximum > 0);
        Assert.True(folded >= maximum - 1e-7);
    }

    private static EnuPosition Position(EnuFrame anchor, int x, int y, short height)
    {
        var p = anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(y / 3600d - 90, x / 3600d - 180, 0)));
        return new EnuPosition(p.East, p.North, p.Up + height);
    }
}
