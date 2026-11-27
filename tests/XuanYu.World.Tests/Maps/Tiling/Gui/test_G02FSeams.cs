using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02FSeams
{
    [Fact]
    public void RealHgt_BuildsOnlyTerrainSurfaceWithoutSkirtWalls()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var source = new NasademElevationSource(hgt.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        GeoTileId id = GeographicTileScheme.Find(
            new GeoCoordinate(24.5, 121.5, 0), 9);
        GeoHeightTile tile = GeoHeightTileBuilder.Build(source, id, 16);
        var mesh = GeoTerrainMeshBuilder.Build(tile, anchor, source, coverage);
        Assert.NotNull(mesh);
        Assert.True(mesh.TriangleCount > 0);
        Assert.DoesNotContain("seam", mesh.AssetId);
    }

    [Fact]
    public void ImportHgt_UsesG03TerrainAsDefaultViewport()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var workbench = new Workbench();
        workbench.ImportTerrain(hgt.Path);
        Assert.True(workbench.IsGeoTerrainPreview);
        Assert.Equal(hgt.Load().TileId, workbench.TerrainSourceTile!.TileId);

        IReadOnlyList<RenderDraw> draws = [];
        for (int i = 0; i < 64; i++)
        {
            draws = workbench.ViewportDraws(1000, 700);
            if (draws.Any(draw => draw.Mesh.AssetId.StartsWith(
                "terrain/g02/", StringComparison.Ordinal))) break;
        }

        var terrains = draws.Where(draw => draw.Mesh.AssetId.StartsWith(
            "terrain/", StringComparison.Ordinal)).ToArray();
        Assert.NotEmpty(terrains);
        Assert.All(terrains, draw =>
            Assert.StartsWith("terrain/g02/", draw.Mesh.AssetId));
        Assert.DoesNotContain(terrains, draw =>
            draw.Mesh.AssetId.Contains("/seam/", StringComparison.Ordinal));
        Assert.DoesNotContain(draws, draw =>
            draw.Mesh.AssetId.StartsWith("demo/", StringComparison.Ordinal));
    }
}