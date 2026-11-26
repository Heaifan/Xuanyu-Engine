using XuanYu.Core.Geodesy;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07GpuCoordinates
{
    [G07DataFact]
    public async Task RealGpuPositionsMatchCpuForRealTerrainPage()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "v1", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var tile = new GeoUnifiedTile(6, 529, 199);
        using var page = await new GeoUnifiedPageLoader(catalog, loader, cache).RequestAsync(tile);
        short[] heights = new short[33 * 33];
        for (int row = 0; row < 33; row++)
            for (int col = 0; col < 33; col++) heights[row * 33 + col] = page.Page.HeightAt(col, row)!.Value;
        var origin = new GeoCoordinate(23.5, 121, 0);
        using var context = new VkContext();
        Assert.True(context.ShaderFloat64Enabled);
        var positions = context.ProbeTerrainCoordinates(tile.South, tile.West, 1 << tile.Level, origin, heights);
        Assert.Equal(33 * 33, positions.Length);
        var anchor = new EnuFrame(origin);
        double max = 0;
        for (int row = 0; row < 33; row++)
            for (int col = 0; col < 33; col++)
            {
                var geo = new GeoCoordinate((tile.South + row * 64) / 3600d - 90,
                    (tile.West + col * 64) / 3600d - 180, 0);
                var p = anchor.ToEnu(Wgs84.ToEcef(geo));
                var gpu = positions[row * 33 + col];
                double x = p.East - gpu.X, y = p.North - gpu.Y, z = p.Up + heights[row * 33 + col] - gpu.Z;
                max = Math.Max(max, Math.Sqrt(x * x + y * y + z * z));
            }
        Assert.InRange(max, 0, 0.10);
    }
}
