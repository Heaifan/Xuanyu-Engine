using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07RealSeam3D
{
    [G07DataFact]
    public async Task FineEdgeMatchesCoarseEdgeNearRealHgtBoundary()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var fine = new GeoUnifiedTile(0, 33861, 12736);
        var coarse = new GeoUnifiedTile(1, 16931, 6368);
        using var f = await pages.RequestAsync(fine);
        using var c = await pages.RequestAsync(coarse);
        int[] indices = GeoUnifiedGridIndices.Create(8);
        int[] edgeRows = indices.Where(i => i % 33 == 32).Select(i => i / 33).Distinct().Order().ToArray();
        Assert.Equal(Enumerable.Range(0, 17).Select(i => i * 2), edgeRows);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        foreach (int row in edgeRows)
        {
            int y = fine.South + row;
            Assert.Equal(f.Page.HeightAt(32, row), c.Page.HeightAt(0, row / 2));
            var a = Position(anchor, fine.West + 32, y, f.Page.HeightAt(32, row)!.Value);
            var b = Position(anchor, coarse.West, coarse.South + row, c.Page.HeightAt(0, row / 2)!.Value);
            double delta = Math.Sqrt(Math.Pow(a.East - b.East, 2) + Math.Pow(a.North - b.North, 2) + Math.Pow(a.Up - b.Up, 2));
            Assert.InRange(delta, 0, 0.01);
        }
    }

    private static EnuPosition Position(EnuFrame anchor, int x, int y, short height)
    {
        var zero = new GeoCoordinate(y / 3600d - 90, x / 3600d - 180, 0);
        var p = anchor.ToEnu(Wgs84.ToEcef(zero));
        return new EnuPosition(p.East, p.North, p.Up + height);
    }
}
