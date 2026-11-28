using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
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
        var mesh = GeoUnifiedMeshAdapter.Build(page.Page, anchor, 0);
        var vertices = mesh.Vertices.ToArray();
        double max = 0, meshMax = 0;
        for (int row = 0; row < 33; row++)
            for (int col = 0; col < 33; col++)
            {
                int i = row * 33 + col, v = i * 7;
                var p = GeoUnifiedPosition.At(anchor, tile.West + col * 64, tile.South + row * 64, heights[i]);
                var gpu = positions[i];
                max = Math.Max(max, Distance(p.East - gpu.X, p.North - gpu.Y, p.Up - gpu.Z));
                meshMax = Math.Max(meshMax, Distance(vertices[v] - gpu.X,
                    vertices[v + 1] - gpu.Y, vertices[v + 2] - gpu.Z));
            }
        Assert.InRange(max, 0, 0.10);
        Assert.InRange(meshMax, 0, 0.10);
    }

    private static double Distance(double x, double y, double z) => Math.Sqrt(x * x + y * y + z * z);
}