using System.Numerics;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.Runtime;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C02RepresentationEditing
{
    [Fact]
    public void PickingUsesActualRepresentationAndSelectionSurvives()
    {
        var scene = new PreviewScene();
        var cube = scene.CreateCube();
        var parent = scene.CreateCube();
        scene.SetOrganizationParent(cube.Id, parent.Id);
        scene.Select(cube.Id);
        var before = scene.World.Find(cube.Id);
        Assert.Equal(cube.Id, Pick(80, 80));
        scene.SetRepresentation(cube.Id, EntityRepresentationKind.Proxy);
        var proxy = Assert.Single(scene.Draws, draw => draw.Selected);
        Assert.Equal("builtin/cube-proxy", proxy.Mesh.AssetId);
        Assert.Equal(8, proxy.Mesh.TriangleCount);
        Assert.Null(Pick(80, 80));
        Assert.Equal(cube.Id, Pick(50, 50));
        scene.SetRepresentation(cube.Id, EntityRepresentationKind.None);
        Assert.Null(Pick(50, 50));
        Assert.DoesNotContain(scene.Draws, draw => draw.Selected);
        Assert.Equal(cube.Id, scene.SelectedId);
        Assert.Equal(before, scene.World.Find(cube.Id));
        Assert.Equal(parent.Id, scene.World.GetOrganizationParent(cube.Id));
        scene.SetRepresentation(cube.Id, EntityRepresentationKind.Mesh);
        Assert.Same(cube.Mesh, Assert.Single(scene.Draws, draw => draw.Selected).Mesh);
        Guid? Pick(int x, int y) => PreviewMeshPicker.Pick(scene, x, y, 100, 100, Matrix4x4.Identity);
    }

    [Fact]
    public void VisibilityAndUnsupportedAssetRemainIndependent()
    {
        var scene = PreviewSeed.Create();
        var other = scene.Objects[0];
        Assert.False(PreviewScene.SupportsRepresentation(other));
        Assert.Throws<InvalidOperationException>(() => scene.SetRepresentation(other.Id, EntityRepresentationKind.Proxy));
        var cube = scene.CreateCube();
        scene.SetVisible(cube.Id, false);
        scene.SetRepresentation(cube.Id, EntityRepresentationKind.Proxy);
        Assert.False(Assert.Single(scene.Draws, d => d.Mesh.AssetId == "builtin/cube-proxy").Visible);
        scene.SetRepresentation(cube.Id, EntityRepresentationKind.None);
        scene.SetRepresentation(cube.Id, EntityRepresentationKind.Mesh);
        Assert.False(cube.Visible);
    }

    [Fact]
    public void PlayCopiesRepresentationWithoutReplacingEditorSelection()
    {
        var wb = new Workbench(use3D: true);
        var cube = wb.Scene.CreateCube();
        wb.Scene.SetRepresentation(cube.Id, EntityRepresentationKind.Proxy);
        wb.StartSimulation();
        Assert.Contains(wb.RenderDraws, d => d.Mesh.AssetId == "builtin/cube-proxy");
        Assert.DoesNotContain(wb.RenderDraws, d => d.Mesh.AssetId == "builtin/cube");
        wb.StopSimulation();
        Assert.Equal(cube.Id, wb.Scene.SelectedId);
        Assert.Equal(EntityRepresentationKind.Proxy, wb.Scene.GetRepresentation(cube.Id));
    }

    [Fact]
    public void SaveKeepsOriginalMeshAndReloadRestoresDefaultView()
    {
        var scene = new PreviewScene(); var cube = scene.CreateCube();
        scene.SetRepresentation(cube.Id, EntityRepresentationKind.None);
        var path = Path.Combine(AppContext.BaseDirectory, $"XYE-C02-{Guid.NewGuid():N}.xyscene");
        try
        {
            SceneFile.Save(scene, path);
            SceneFile.Load(scene, path);
            Assert.Equal(cube.Id, Assert.Single(scene.Objects).Id);
            Assert.Equal(EntityRepresentationKind.Mesh, scene.GetRepresentation(cube.Id));
            Assert.Equal("builtin/cube", Assert.Single(scene.Draws).Mesh.AssetId);
        }
        finally { File.Delete(path); }
    }
}