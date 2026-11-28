using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_SceneMenuOperations
{
    [Fact]
    public void Rename_ShouldUpdateWorldAndObjectWithoutChangingId()
    {
        var scene = PreviewSeed.Create3D();
        PreviewObject cube = Assert.Single(scene.Objects);
        int notifications = 0;
        scene.EntityRenamed += () => notifications++;
        scene.Select(cube.Id);

        scene.Rename(cube.Id, " Cube_Main ");

        Assert.Equal(cube.Id, scene.SelectedId);
        Assert.Equal("Cube_Main", cube.Name);
        Assert.Equal("Cube_Main", scene.World.Find(cube.Id)!.Name);
        Assert.Equal(1, notifications);
        Assert.Single(scene.ViewportDraws(800, 600));
    }

    [Fact]
    public void Rename_ShouldRejectBlankAndDuplicateNames()
    {
        var scene = PreviewSeed.Create3D();
        var cube = scene.CreateCube();

        Assert.Throws<ArgumentException>(() => scene.Rename(cube.Id, " "));
        Assert.Throws<InvalidOperationException>(
            () => scene.Rename(cube.Id, "Cube_001"));
        Assert.Equal("Cube_002", scene.World.Find(cube.Id)!.Name);
    }

    [Fact]
    public void DeleteSelected_ShouldClearSelectionAndDrawing()
    {
        var scene = PreviewSeed.Create3D();
        var cube = scene.CreateCube();

        Assert.True(scene.Delete(cube.Id));

        Assert.Null(scene.SelectedId);
        Assert.Null(scene.World.Find(cube.Id));
        Assert.Single(scene.Objects);
        Assert.Single(scene.ViewportDraws(800, 600));
    }
}