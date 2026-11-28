using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Render;
using XuanYu.World.Tests.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E601FR01RuntimeEnvironment
{
    private const string Sky = "editor/sky-gradient";
    private const string Grid = "editor/metric-grid";

    [Fact]
    public void PlayStop_RetainEnvironmentAndHonorSwitches()
    {
        var wb = new Workbench(use3D: true);
        wb.Scene.CreateCube();
        var before = wb.ViewportDraws(800, 600);
        wb.StartSimulation();
        var runtime = wb.ViewportDraws(800, 600);
        Assert.Equal(before.Select(d => d.Mesh.AssetId), runtime.Select(d => d.Mesh.AssetId));
        Assert.Equal(Sky, runtime[0].Mesh.AssetId);
        Assert.Equal(Grid, runtime[^1].Mesh.AssetId);
        Assert.Single(runtime, d => d.Mesh.AssetId == "builtin/cube");
        Assert.Empty(wb.ViewportDraws(1, 600));
        wb.SetSkyEnabled(false);
        Assert.DoesNotContain(wb.ViewportDraws(800, 600), d => d.Mesh.AssetId == Sky);
        Assert.Single(wb.ViewportDraws(800, 600), d => d.Mesh.AssetId == Grid);
        wb.SetWorldGridEnabled(false);
        Assert.DoesNotContain(wb.ViewportDraws(800, 600), d => d.Mesh.AssetId == Grid);
        wb.SetSkyEnabled(true);
        wb.SetWorldGridEnabled(true);
        wb.StopSimulation();
        Assert.Equal(before.Select(d => d.Mesh.AssetId),
            wb.ViewportDraws(800, 600).Select(d => d.Mesh.AssetId));
    }

    [Fact]
    public void RuntimeCamera_DrivesGridAndSkyWithoutChangingEditorCamera()
    {
        var wb = new Workbench(use3D: true);
        var editor = wb.ActiveCamera!;
        var projection = editor.ViewProjection(800, 600);
        var sky = Find(wb, Sky).ModelToClip;
        wb.StartSimulation();
        var camera = wb.ActiveCamera!;
        camera.Orbit(70f, 20f);
        camera.Zoom(1f);
        var moved = camera.ViewProjection(800, 600);
        Assert.NotEqual(projection, moved);
        Assert.Equal(moved, Find(wb, Grid).ModelToClip);
        Assert.NotEqual(sky, Find(wb, Sky).ModelToClip);
        Assert.Equal(projection, editor.ViewProjection(800, 600));
        wb.StopSimulation();
        Assert.Equal(projection, Find(wb, Grid).ModelToClip);
        Assert.Equal(sky, Find(wb, Sky).ModelToClip);
    }

    [Fact]
    public void ImportedDem_G07SourceRemainsAcrossPlayStopAndPageSwitch()
    {
        using var file = new G02BHgtFixture("N24E121");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(file.Path);
        var source = wb.G07PreviewSource;
        Assert.NotNull(source);
        Assert.Null(wb.ActiveCamera);
        wb.StartSimulation();
        Assert.Same(source, wb.G07PreviewSource);
        Assert.Null(wb.ActiveCamera);
        wb.SwitchTo(PageId.Terrain);
        Assert.Same(source, wb.G07PreviewSource);
        wb.SwitchTo(PageId.World);
        wb.StopSimulation();
        Assert.Same(source, wb.G07PreviewSource);
        Assert.Null(wb.ActiveCamera);
    }

    private static RenderDraw Find(Workbench wb, string asset) =>
        Assert.Single(wb.ViewportDraws(800, 600), d => d.Mesh.AssetId == asset);
}