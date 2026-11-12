using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_E205WorkbenchSession
{
    [Fact]
    public void RoundTrip_ShouldRestoreWorldViewportAndFurtherEditing()
    {
        string path = ScenePath();
        try
        {
            var source = new Workbench(use3D: true);
            var first = Assert.Single(source.Scene.Objects);
            var second = source.Scene.CreateCube();
            source.Scene.Rename(first.Id, "MainCube");
            var transform = new Transform(
                new Vector3(2.5f, -1f, 3f),
                Quaternion.CreateFromYawPitchRoll(0.4f, 0.2f, 0.1f),
                new Vector3(2f, 1.5f, 0.5f));
            source.Scene.SetTransform(first.Id, transform);
            source.Scene.SetVisible(second.Id, false);
            source.Scene.Select(second.Id);
            SceneFile.Save(source.Scene, path);

            var reopened = new Workbench(use3D: true);
            SceneFile.Load(reopened.Scene, path);
            Assert.Equal(2, reopened.Scene.Objects.Count);
            Assert.Equal(first.Id, reopened.Scene.Objects[0].Id);
            Assert.Equal(second.Id, reopened.Scene.Objects[1].Id);
            Assert.Equal("MainCube", reopened.Scene.Objects[0].Name);
            Assert.Equal(transform, reopened.World.Find(first.Id)!.Transform);
            Assert.False(reopened.World.Find(second.Id)!.IsVisible);
            Assert.Null(reopened.Scene.SelectedId);
            var draws = reopened.Scene.ViewportDraws(800, 600);
            Assert.Equal(2, draws.Count);
            Assert.True(draws[0].Visible);
            Assert.False(draws[1].Visible);
            reopened.Scene.Select(first.Id);
            reopened.Scene.SetTransform(first.Id, Transform.Identity);
            Assert.Equal(Transform.Identity, reopened.World.Find(first.Id)!.Transform);
            Assert.True(reopened.Scene.ViewportDraws(800, 600)[0].Selected);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Open_ShouldReplaceOldSceneAndAllowNewCube()
    {
        string path = ScenePath();
        try
        {
            var source = new Workbench(use3D: true);
            source.Scene.CreateCube();
            SceneFile.Save(source.Scene, path);

            var target = new Workbench(use3D: true);
            target.Scene.CreateCube();
            Guid staleId = target.Scene.CreateCube().Id;
            Assert.Equal(3, target.Scene.Objects.Count);
            SceneFile.Load(target.Scene, path);
            Assert.Null(target.World.Find(staleId));
            Assert.Equal(2, target.Scene.Objects.Count);
            var added = target.Scene.CreateCube();
            Assert.Equal("Cube_003", added.Name);
            Assert.NotNull(target.World.Find(added.Id));
            Assert.Equal(3, target.Scene.ViewportDraws(800, 600).Count);
            Assert.Equal(added.Id, target.Scene.SelectedId);
        }
        finally { File.Delete(path); }
    }

    private static string ScenePath() => Path.Combine(
        AppContext.BaseDirectory, $"XYE-E205-{Guid.NewGuid():N}.xyscene");
}