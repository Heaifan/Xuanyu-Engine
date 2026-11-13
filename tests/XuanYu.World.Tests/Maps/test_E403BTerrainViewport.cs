
using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests.Maps;

public sealed class TestE403BTerrainViewport
{
    [Fact]
    public void TerrainPage_UsesTerrainDraws()
    {
        var wb = new Workbench(use3D: true);

        Assert.Equal(36,
            Assert.Single(wb.ViewportDraws(800, 600))
                .Mesh.VertexCount);

        wb.SwitchTo(PageId.Terrain);

        var draw = Assert.Single(
            wb.ViewportDraws(800, 600));

        Assert.Equal(98304, draw.Mesh.VertexCount);
        Assert.Same(wb.TerrainScene.Camera, wb.ActiveCamera);
    }

    [Fact]
    public void CameraOrbit_DoesNotRebuildTerrain()
    {
        var wb = new Workbench(use3D: true);
        wb.SwitchTo(PageId.Terrain);

        var before = Assert.Single(
            wb.ViewportDraws(1200, 800));
        var camera = wb.ActiveCamera!;

        camera.Orbit(150f, 50f);
        camera.Zoom(3f);

        var after = Assert.Single(
            wb.ViewportDraws(800, 600));

        Assert.Same(before.Mesh, after.Mesh);
        Assert.NotEqual(before.ModelToClip, after.ModelToClip);
    }

    [Fact]
    public void SwitchingPages_PreservesWorldAndTerrain()
    {
        var wb = new Workbench(use3D: true);

        var worldMesh = Assert.Single(
            wb.ViewportDraws(800, 600)).Mesh;

        wb.SwitchTo(PageId.Terrain);
        var terrainMesh = Assert.Single(
            wb.ViewportDraws(800, 600)).Mesh;

        wb.SwitchTo(PageId.World);
        Assert.Same(worldMesh,
            Assert.Single(wb.ViewportDraws(800, 600)).Mesh);

        wb.SwitchTo(PageId.Terrain);
        Assert.Same(terrainMesh,
            Assert.Single(wb.ViewportDraws(800, 600)).Mesh);
    }
}
