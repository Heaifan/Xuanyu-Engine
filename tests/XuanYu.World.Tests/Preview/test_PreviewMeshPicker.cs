
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;

namespace XuanYu.World.Tests;

public sealed class test_PreviewMeshPicker
{
    [Fact]
    public void ClickMesh_SelectsCorrespondingObjectAndNotifiesWorkbench()
    {
        var workbench = new Workbench();
        int notifications = 0;
        workbench.Changed += () => notifications++;

        Guid? triangle = PreviewMeshPicker.Pick(
            workbench.Scene, 208, 300, 756, 580);
        workbench.Scene.Select(triangle);
        Assert.Equal(workbench.Scene.Objects[0].Id, workbench.Scene.SelectedId);
        Assert.Equal(1, notifications);

        Guid? quad = PreviewMeshPicker.Pick(
            workbench.Scene, 548, 290, 756, 580);
        workbench.Scene.Select(quad);
        Assert.Equal(workbench.Scene.Objects[1].Id, workbench.Scene.SelectedId);
        Assert.Equal(2, notifications);
    }

    [Fact]
    public void HiddenObjectCannotBePicked()
    {
        var scene = PreviewSeed.Create();
        scene.SetVisible(scene.Objects[1].Id, false);

        Assert.Null(PreviewMeshPicker.Pick(scene, 548, 290, 756, 580));
        Assert.Equal(scene.Objects[0].Id,
            PreviewMeshPicker.Pick(scene, 208, 300, 756, 580));
    }

    [Fact]
    public void ClickBackgroundReturnsNoSelection()
    {
        var scene = PreviewSeed.Create();
        Assert.Null(PreviewMeshPicker.Pick(scene, 20, 20, 756, 580));
    }
}
