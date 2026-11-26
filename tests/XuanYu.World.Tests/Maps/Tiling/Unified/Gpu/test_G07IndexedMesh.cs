using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07IndexedMesh
{
    [G07DataFact]
    public async Task RealPageBuildsSharedIndexedMeshWithOriginalHeights()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var cache = new GeoUnifiedPageCache(loader, new GeoUnifiedSamples(catalog, "v1", loader));
        var tile = new GeoUnifiedTile(6, 529, 199);
        using var page = await new GeoUnifiedPageLoader(catalog, loader, cache).RequestAsync(tile);
        var mesh = GeoUnifiedMeshAdapter.Build(page.Page, new EnuFrame(new GeoCoordinate(23.5, 121, 0)), 15);
        Assert.Equal(1089, mesh.VertexCount);
        Assert.Equal(GeoUnifiedGridIndices.Create(15).Length, mesh.IndexCount);
        Assert.Equal(GeoUnifiedGridIndices.Create(15).Length / 3, mesh.TriangleCount);
        Assert.All(mesh.Vertices.ToArray(), value => Assert.True(float.IsFinite(value)));
    }
}
