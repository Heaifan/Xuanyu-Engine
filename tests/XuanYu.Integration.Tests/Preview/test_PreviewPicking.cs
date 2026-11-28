
using System.Numerics;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_PreviewPicking
{
    [Fact]
    public void ClickMesh_SelectsSingleSceneOwner()
    {
        var scene = NewScene(out var item);
        Guid? hit = PreviewMeshPicker.Pick(scene, 50, 50, 100, 100);

        Assert.Equal(item.Id, hit);
        scene.Select(hit);
        Assert.Equal(item.Id, scene.SelectedId);
        Assert.Same(item, scene.Selected);
        Assert.True(scene.Draws[0].Selected);
    }

    [Fact]
    public void ClickBlank_ClearsSelectionAndRenderFlag()
    {
        var scene = NewScene(out var item);
        scene.Select(item.Id);
        Guid? hit = PreviewMeshPicker.Pick(scene, 5, 5, 100, 100);

        Assert.Null(hit);
        scene.Select(hit);
        Assert.Null(scene.Selected);
        Assert.False(scene.Draws[0].Selected);
    }

    [Fact]
    public void HiddenMesh_CannotBePicked()
    {
        var scene = NewScene(out var item);
        scene.SetVisible(item.Id, false);

        Assert.Null(PreviewMeshPicker.Pick(scene, 50, 50, 100, 100));
        Assert.False(scene.Draws[0].Visible);
        scene.SetVisible(item.Id, true);
        Assert.Equal(item.Id, PreviewMeshPicker.Pick(scene, 50, 50, 100, 100));
    }

    [Fact]
    public void SelectionChange_PublishesOncePerActualChange()
    {
        var scene = NewScene(out var item);
        int count = 0;
        scene.Changed += () => count++;

        scene.Select(item.Id);
        scene.Select(item.Id);
        scene.Select(null);

        Assert.Equal(2, count);
    }

    private static PreviewScene NewScene(out PreviewObject item)
    {
        float[] vertices =
        [
            -0.8f, -0.8f, 0f, 1f, 0f, 0f, 1f,
             0.8f, -0.8f, 0f, 0f, 1f, 0f, 1f,
             0f,    0.8f, 0f, 0f, 0f, 1f, 1f
        ];
        var scene = new PreviewScene();
        item = scene.Add("测试三角形", new RenderMesh("test/triangle", vertices),
            Matrix4x4.Identity);
        return scene;
    }
}
