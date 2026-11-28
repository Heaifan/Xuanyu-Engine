using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_PreviewWorldBridge
{
    [Fact]
    public void PreviewObject_ShouldUseWorldEntityIdentity()
    {
        var scene = new PreviewScene();
        var item = scene.Add("Cube", Mesh(),
            Matrix4x4.CreateTranslation(0.3f, 0f, 0f));

        WorldEntity entity = Assert.Single(scene.World.Entities);

        Assert.Equal(entity.Id, item.Id);
        Assert.Equal(entity.Name, item.Name);
        Assert.Equal(0.3f, entity.Transform.Position.X);
        Assert.Equal(0.3f, scene.Draws[0].ModelToClip.M41);
    }

    [Fact]
    public void WorldTransform_ShouldUpdateRenderAndPickingMatrix()
    {
        var scene = new PreviewScene();
        var item = scene.Add("Cube", Mesh(), Matrix4x4.Identity);

        scene.World.SetTransform(item.Id, new Transform(
            new Vector3(2.5f, 0f, 1f),
            Quaternion.Identity, Vector3.One));

        Assert.Equal(2.5f, scene.Draws[0].ModelToClip.M41);
        Assert.Equal(1f, item.ModelToClip.M43);
    }

    [Fact]
    public void VisibilitySelectionAndDelete_ShouldStayConsistent()
    {
        var scene = new PreviewScene();
        var item = scene.Add("Cube", Mesh(), Matrix4x4.Identity);

        scene.Select(item.Id);
        scene.SetVisible(item.Id, false);

        RenderDraw draw = Assert.Single(scene.Draws);
        Assert.False(draw.Visible);
        Assert.True(draw.Selected);
        Assert.False(scene.World.Find(item.Id)!.IsVisible);

        Assert.True(scene.Delete(item.Id));
        Assert.Empty(scene.Draws);
        Assert.Empty(scene.Objects);
        Assert.Null(scene.SelectedId);
        Assert.Null(scene.World.Find(item.Id));
    }

    [Fact]
    public void Workbench_ShouldRenderFromWorldScene()
    {
        var wb = new Workbench();
        var first = wb.Scene.Objects[0];

        wb.World.SetTransform(first.Id, new Transform(
            new Vector3(0.25f, 0f, 0f),
            Quaternion.Identity, Vector3.One));

        Assert.Equal(0.25f, wb.RenderDraws[0].ModelToClip.M41,
            5);
    }

    private static RenderMesh Mesh() =>
        new("test/bridge", new float[21]);
}