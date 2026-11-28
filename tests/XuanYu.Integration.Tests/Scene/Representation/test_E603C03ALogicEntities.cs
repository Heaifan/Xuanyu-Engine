using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Runtime;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests;

public sealed class test_E603C03ALogicEntities
{
    [Fact]
    public void MeshlessIdentitySelectionHierarchyAndDeletion()
    {
        var scene = new PreviewScene();
        var item = scene.CreateLogicEntity();
        Assert.Null(item.Mesh);
        Assert.Equal(item.Id, Assert.Single(scene.World.Entities).Id);
        Assert.Equal(item.Id, scene.SelectedId);
        Assert.Equal(EntityRepresentationKind.None, scene.GetRepresentation(item.Id));
        scene.SetRepresentation(item.Id, EntityRepresentationKind.None);
        Assert.Throws<InvalidOperationException>(() => scene.SetRepresentation(item.Id, EntityRepresentationKind.Mesh));
        Assert.Empty(scene.Draws);
        Assert.Null(PreviewMeshPicker.Pick(scene, 50, 50, 100, 100, Matrix4x4.Identity));
        var cube = scene.CreateCube();
        Assert.True(scene.SetOrganizationParent(cube.Id, item.Id));
        scene.Rename(item.Id, "Group");
        Assert.Equal("Group", item.Name);
        Assert.Equal(item.Id, scene.World.GetOrganizationParent(cube.Id));
        Assert.Single(scene.Draws);
        scene.Select(item.Id);
        Assert.True(scene.Delete(item.Id));
        Assert.Null(scene.SelectedId);
        Assert.Null(scene.World.Find(item.Id));
        Assert.Null(scene.World.GetOrganizationParent(cube.Id));
        Assert.Equal(cube.Id, Assert.Single(scene.Objects).Id);
    }

    [Fact]
    public void TransformHistoryAndFocusUseEntityOrigin()
    {
        var scene = new PreviewScene();
        var item = scene.CreateLogicEntity(SpatialPosition.FromLocal(2, 3, 4));
        var before = scene.World.Find(item.Id)!.Transform;
        var next = new Transform(new Vector3(10, 20, 30), Quaternion.Identity, Vector3.One);
        scene.SetTransform(item.Id, next);
        scene.UndoTransform();
        Assert.Equal(before, scene.World.Find(item.Id)!.Transform);
        scene.RedoTransform();
        Assert.Equal(next, scene.World.Find(item.Id)!.Transform);
        var camera = new ViewportCamera();
        Assert.True(ViewportSceneFraming.Focus(camera, item, 1f));
        Assert.Equal(next.Position, camera.Target);
        scene.SetVisible(item.Id, false);
        Assert.False(ViewportSceneFraming.Focus(camera, item, 1f));
        Assert.Empty(scene.Draws);
    }

    [Fact]
    public void GeographicLogicEntityProducesNoDraws()
    {
        var scene = new PreviewScene();
        var anchor = new EnuFrame(new GeoCoordinate(23, 120, 10));
        var position = SpatialPosition.FromEcef(anchor.OriginEcef);
        var item = scene.CreateLogicEntity(position);
        var frame = new GeoRenderFrame(anchor, anchor.ToEcef(new(0, -20, 10)),
            anchor.OriginEcef, 1f, .1f, 1000f);
        Assert.True(item.IsGeographic);
        Assert.Equal(position, scene.World.GetSpatialPosition(item.Id));
        Assert.Null(item.Mesh);
        Assert.Empty(scene.RenderGeographic(frame));
        Assert.Empty(scene.Draws);
    }

    [Fact]
    public void SimulationClonesLogicEntityAndKeepsEditorState()
    {
        var wb = new Workbench(use3D: true);
        var item = wb.Scene.CreateLogicEntity();
        var cube = wb.Scene.CreateCube();
        wb.Scene.SetOrganizationParent(cube.Id, item.Id);
        var before = wb.RenderDraws.ToArray();
        wb.StartSimulation();
        var runtime = wb.Simulation.RunningWorld!;
        Assert.Equal(wb.World.Find(item.Id), runtime.Find(item.Id));
        Assert.Equal(item.Id, runtime.GetOrganizationParent(cube.Id));
        Assert.Equal(before.Select(d => d.Mesh), wb.RenderDraws.Select(d => d.Mesh));
        runtime.Rename(item.Id, "Runtime");
        wb.StopSimulation();
        Assert.Equal("Entity_001", item.Name);
        Assert.Equal(item.Id, wb.World.GetOrganizationParent(cube.Id));
    }
}