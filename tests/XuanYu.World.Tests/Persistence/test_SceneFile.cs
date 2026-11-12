using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_SceneFile
{
    [Fact]
    public void SaveLoad_ShouldRestoreIdsMeshAndTransform()
    {
        string path = TestPath();
        try
        {
            PreviewScene source = PreviewSeed.Create3D();
            PreviewObject first = source.Objects[0];
            source.Rename(first.Id, "MainCube");
            source.SetVisible(first.Id, false);
            var transform = new Transform(new Vector3(2.5f, -1f, 3f),
                Quaternion.CreateFromYawPitchRoll(0.4f, 0.2f, 0.1f),
                new Vector3(2f, 1.5f, 0.5f));
            source.SetTransform(first.Id, transform);
            PreviewObject second = source.CreateCube();
            source.Select(second.Id);
            SceneFile.Save(source, path);

            PreviewScene restored = PreviewSeed.Create3D();
            SceneFile.Load(restored, path);

            Assert.Equal(2, restored.Objects.Count);
            Assert.Equal(first.Id, restored.Objects[0].Id);
            Assert.Equal(second.Id, restored.Objects[1].Id);
            Assert.Equal("MainCube", restored.Objects[0].Name);
            Assert.Equal("builtin/cube", restored.Objects[0].Mesh.AssetId);
            Assert.False(restored.Objects[0].Visible);
            Assert.Equal(transform, restored.World.Find(first.Id)!.Transform);
            Assert.Null(restored.SelectedId);
            Assert.Equal(2, restored.ViewportDraws(800, 600).Count);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void InvalidJson_ShouldLeaveCurrentSceneUntouched()
    {
        string path = TestPath();
        try
        {
            PreviewScene scene = PreviewSeed.Create3D();
            WorldEntity before = Assert.Single(scene.World.Entities);
            File.WriteAllText(path, "{invalid-json");

            Assert.Throws<InvalidDataException>(() => SceneFile.Load(scene, path));
            Assert.Equal(before, Assert.Single(scene.World.Entities));
            Assert.Equal(before.Id, Assert.Single(scene.Objects).Id);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void UnknownMesh_ShouldRejectBeforeReplacingScene()
    {
        string path = TestPath();
        try
        {
            PreviewScene scene = PreviewSeed.Create3D();
            WorldEntity before = Assert.Single(scene.World.Entities);
            SceneFile.Save(scene, path);
            string json = File.ReadAllText(path).Replace(
                "builtin/cube", "unknown/mesh", StringComparison.Ordinal);
            File.WriteAllText(path, json);

            Assert.Throws<InvalidDataException>(() => SceneFile.Load(scene, path));
            Assert.Equal(before, Assert.Single(scene.World.Entities));
            Assert.Equal(before.Id, Assert.Single(scene.Objects).Id);
        }
        finally { File.Delete(path); }
    }

    private static string TestPath() => Path.Combine(
        AppContext.BaseDirectory, $"XYE-E204-{Guid.NewGuid():N}.xyscene");
}