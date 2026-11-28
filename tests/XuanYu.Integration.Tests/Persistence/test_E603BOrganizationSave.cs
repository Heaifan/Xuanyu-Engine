using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;

namespace XuanYu.World.Tests;

public sealed class test_E603BOrganizationSave
{
    [Fact]
    public void V2_RoundTripRestoresHierarchyAndReplacesOldRelations()
    {
        string path = TestPath();
        try
        {
            var source = PreviewSeed.Create3D();
            Guid root = Assert.Single(source.Objects).Id;
            Guid middle = source.CreateCube().Id;
            Guid leaf = source.CreateCube().Id;
            Guid independent = source.CreateCube().Id;
            source.World.SetOrganizationParent(middle, root);
            source.World.SetOrganizationParent(leaf, middle);
            source.SetVisible(middle, false);
            SceneFile.Save(source, path);
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(2, json.RootElement.GetProperty("Version").GetInt32());
            var target = PreviewSeed.Create3D();
            Guid old = Assert.Single(target.Objects).Id;
            target.World.SetOrganizationParent(target.CreateCube().Id, old);
            SceneFile.Load(target, path);
            Assert.Equal(source.World.Entities.ToArray(), target.World.Entities.ToArray());
            Assert.Equal(source.Objects.Select(x => x.Id), target.Objects.Select(x => x.Id));
            Assert.Null(target.World.Find(old));
            Assert.Null(target.World.GetOrganizationParent(root));
            Assert.Null(target.World.GetOrganizationParent(independent));
            Assert.Equal(root, target.World.GetOrganizationParent(middle));
            Assert.Equal(middle, target.World.GetOrganizationParent(leaf));
            Assert.Equal(middle, Assert.Single(target.World.GetOrganizationChildren(root)));
            Assert.Equal(leaf, Assert.Single(target.World.GetOrganizationChildren(middle)));
            Assert.False(target.Objects.Single(x => x.Id == middle).Visible);
            Assert.Null(target.SelectedId);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void V1_WithoutParentFieldsLoadsAsIndependentEntities()
    {
        string path = TestPath();
        try
        {
            var source = PreviewSeed.Create3D();
            source.CreateCube();
            SceneFile.Save(source, path);
            var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            json["Version"] = 1;
            foreach (var entity in json["Entities"]!.AsArray())
                entity!.AsObject().Remove("OrganizationParentId");
            File.WriteAllText(path, json.ToJsonString());
            var target = PreviewSeed.Create3D();
            Guid old = Assert.Single(target.Objects).Id;
            target.World.SetOrganizationParent(target.CreateCube().Id, old);
            SceneFile.Load(target, path);
            Assert.Equal(source.World.Entities.ToArray(), target.World.Entities.ToArray());
            Assert.False(target.World.HasOrganizationRelations);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Save_RejectsUnrepresentedOrganizationEntityBeforeOverwritingFile()
    {
        string path = TestPath();
        try
        {
            var scene = PreviewSeed.Create3D();
            Guid child = Assert.Single(scene.Objects).Id;
            SceneFile.Save(scene, path);
            string before = File.ReadAllText(path);
            Guid parent = scene.World.Create("NoMesh").Id;
            scene.World.SetOrganizationParent(child, parent);
            Assert.Throws<InvalidOperationException>(() => SceneFile.Save(scene, path));
            Assert.Equal(before, File.ReadAllText(path));
        }
        finally { File.Delete(path); }
    }

    private static string TestPath() => Path.Combine(AppContext.BaseDirectory,
        $"XYE-E603B02-{Guid.NewGuid():N}.xyscene");
}