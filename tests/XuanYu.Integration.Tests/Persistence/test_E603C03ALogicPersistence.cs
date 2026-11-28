using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.Runtime;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests;

public sealed class test_E603C03ALogicPersistence
{
    [Fact]
    public void V3RoundTripRestoresMeshlessIdentityHierarchyAndVisibility()
    {
        string path = TestPath();
        try
        {
            var scene = new PreviewScene();
            var parent = scene.CreateLogicEntity(SpatialPosition.FromLocal(10, 20, 30));
            var cube = scene.CreateCube();
            scene.SetOrganizationParent(cube.Id, parent.Id);
            scene.SetVisible(parent.Id, false);
            SceneFile.Save(scene, path);
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(3, json.RootElement.GetProperty("Version").GetInt32());
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("Entities")[0].GetProperty("MeshId").ValueKind);
            var target = PreviewSeed.Create3D();
            SceneFile.Load(target, path);
            Assert.Equal(scene.World.Entities.ToArray(), target.World.Entities.ToArray());
            var restored = Assert.Single(target.Objects, x => x.Id == parent.Id);
            Assert.Null(restored.Mesh);
            Assert.False(restored.Visible);
            Assert.Equal(parent.Id, target.World.GetOrganizationParent(cube.Id));
            Assert.Equal(EntityRepresentationKind.None, target.GetRepresentation(parent.Id));
            Assert.Equal("builtin/cube", Assert.Single(target.Draws).Mesh.AssetId);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void LegacyVersionRejectsNullMeshWithoutReplacingScene(int version)
    {
        string path = TestPath();
        try
        {
            var scene = new PreviewScene();
            scene.CreateLogicEntity();
            SceneFile.Save(scene, path);
            var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            json["Version"] = version;
            File.WriteAllText(path, json.ToJsonString());
            var target = PreviewSeed.Create3D();
            var before = Assert.Single(target.Objects);
            Assert.Throws<InvalidDataException>(() => SceneFile.Load(target, path));
            Assert.Same(before, Assert.Single(target.Objects));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void SaveRejectsUntrackedEntityEvenWithoutOrganization()
    {
        string path = TestPath();
        try
        {
            var scene = PreviewSeed.Create3D();
            SceneFile.Save(scene, path);
            string before = File.ReadAllText(path);
            scene.World.Create("Untracked");
            Assert.Throws<InvalidOperationException>(() => SceneFile.Save(scene, path));
            Assert.Equal(before, File.ReadAllText(path));
        }
        finally { File.Delete(path); }
    }

    private static string TestPath() => Path.Combine(AppContext.BaseDirectory,
        $"XYE-C03A-{Guid.NewGuid():N}.xyscene");
}