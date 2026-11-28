using System.Linq;
using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests.ViewportReference;

public sealed class test_E5UX01R02LocalViewport
{
    [Fact]
    public void FivePages_KeepLocalSceneCameraAndDraws()
    {
        var wb = new Workbench(use3D: true);
        var camera = wb.ActiveCamera!;
        camera.Pan(80f, 25f, 600);
        var pose = camera.Position;
        var target = camera.Target;
        var baseline = wb.ViewportDraws(800, 600)
            .Select(d => (d.Mesh.AssetId, d.ModelToClip)).ToArray();
        Assert.Contains(baseline, d => d.AssetId == "editor/metric-grid");
        foreach (var page in Enum.GetValues<PageId>())
        {
            wb.SwitchTo(page);
            Assert.Same(wb.Scene, wb.TerrainScene);
            Assert.Same(camera, wb.ActiveCamera);
            Assert.Equal(pose, camera.Position);
            Assert.Equal(target, camera.Target);
            Assert.Equal(camera.ViewProjection(800, 600), wb.EditorProjection(800, 600));
            Assert.Equal(baseline, wb.ViewportDraws(800, 600)
                .Select(d => (d.Mesh.AssetId, d.ModelToClip)).ToArray());
        }
    }

    [Fact]
    public void FivePages_RespectSkyAndIndependentGridSwitches()
    {
        var wb = new Workbench(use3D: true);
        wb.SetWorldGridEnabled(false);
        foreach (var page in Enum.GetValues<PageId>())
        {
            wb.SwitchTo(page);
            wb.SetSkyEnabled(true);
            var withSky = wb.ViewportDraws(800, 600);
            wb.SetSkyEnabled(false);
            var withoutSky = wb.ViewportDraws(800, 600);
            Assert.Equal(withoutSky.Count + 1, withSky.Count);
            Assert.Equal(page == PageId.Terrain, withoutSky.Any(d =>
                d.Mesh.AssetId == "editor/metric-grid"));
        }
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainGridEnabled(false);
        Assert.DoesNotContain(wb.ViewportDraws(800, 600),
            d => d.Mesh.AssetId == "editor/metric-grid");
    }

    [Fact]
    public void ZoomFromProject_ChangesTheSameTerrainCamera()
    {
        var wb = new Workbench(PageId.Project, use3D: true);
        var camera = wb.ActiveCamera!;
        var before = Vector3.Distance(camera.Position, camera.Target);
        wb.ZoomTerrainAt(1f, 400f, 300f, 800, 600);
        Assert.NotEqual(before, Vector3.Distance(camera.Position, camera.Target));
        wb.SwitchTo(PageId.Terrain);
        Assert.Same(camera, wb.ActiveCamera);
    }
}