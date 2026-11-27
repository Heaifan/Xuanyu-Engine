using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class TestE403BTerrainViewport
{
    [Fact]
    public void TerrainPage_UsesSharedLocalTerrainDraws()
    {
        var wb = new Workbench(use3D: true);
        var world = Terrain(wb);
        Assert.Equal(60000, world.Mesh.VertexCount);
        wb.SwitchTo(PageId.Terrain);
        var terrain = Terrain(wb);
        Assert.Same(world.Mesh, terrain.Mesh);
        Assert.Same(wb.Scene, wb.TerrainScene);
        Assert.Same(wb.Scene.Camera, wb.ActiveCamera);
    }

    [Fact]
    public void CameraOrbit_DoesNotRebuildTerrain()
    {
        var wb = new Workbench(use3D: true);
        wb.SwitchTo(PageId.Terrain);
        var before = Terrain(wb, 1200, 800);
        var camera = wb.ActiveCamera!;
        camera.Orbit(150f, 50f);
        camera.Zoom(3f);
        var after = Terrain(wb);
        Assert.Same(before.Mesh, after.Mesh);
        Assert.NotEqual(before.ModelToClip, after.ModelToClip);
    }

    [Fact]
    public void SwitchingPages_PreservesWorldAndTerrain()
    {
        var wb = new Workbench(use3D: true);
        var scene = wb.Scene;
        var camera = scene.Camera;
        var mesh = Terrain(wb).Mesh;
        wb.SwitchTo(PageId.Terrain);
        Assert.Same(mesh, Terrain(wb).Mesh);
        wb.SwitchTo(PageId.World);
        Assert.Same(mesh, Terrain(wb).Mesh);
        Assert.Same(scene, wb.TerrainScene);
        Assert.Same(camera, wb.ActiveCamera);
    }

    private static RenderDraw Terrain(Workbench wb, uint width = 800, uint height = 600) =>
        Assert.Single(wb.ViewportDraws(width, height), draw =>
            draw.Mesh.AssetId.StartsWith("terrain/local/", StringComparison.Ordinal));
}