using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests;

public sealed class test_E602BR01TerrainPersistence
{
    [Fact]
    public void DeletedTerrain_CubeOnlySceneSavesAndReloads()
    {
        var session = new TerrainPreviewSession(TerrainPreviewMap.CreateBlank(), centered: true);
        var scene = session.Scene;
        Guid terrain = Assert.Single(scene.Objects).Id;
        Assert.True(scene.Delete(terrain));
        var cube = scene.CreateCube();
        var transform = new Transform(new Vector3(-3.1f, 0, 0),
            Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathF.PI / 6),
            new Vector3(3.7f, 1, 1));
        scene.SetTransform(cube.Id, transform);
        var position = SpatialPosition.FromLocal(-3.10000001, 0, 0);
        scene.World.SetSpatialPosition(cube.Id, position);
        string path = TestPath();
        try
        {
            SceneFile.Save(scene, path);
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("LocalTerrain").ValueKind);
            var restored = PreviewSeed.Create3D();
            SceneFile.Load(restored, path);
            Assert.Equal(cube.Id, Assert.Single(restored.Objects).Id);
            Assert.Null(restored.World.Find(terrain));
            Assert.Equal(position, restored.World.GetSpatialPosition(cube.Id));
            Assert.Equal(transform.Rotation, restored.World.Find(cube.Id)!.Transform.Rotation);
            Assert.Equal(transform.Scale, restored.World.Find(cube.Id)!.Transform.Scale);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void DeletedCube_RetainedTerrainAndEditedHeightsRoundTrip()
    {
        var map = TerrainPreviewMap.CreateBlank(4, 4);
        map.HeightField.SetHeight(2, 3, 17.25f);
        var scene = new TerrainPreviewSession(map, centered: true).Scene;
        Guid terrain = Assert.Single(scene.Objects).Id;
        Assert.True(scene.Delete(scene.CreateCube().Id));
        string path = TestPath();
        try
        {
            SceneFile.Save(scene, path);
            var restored = PreviewSeed.Create3D();
            SceneFile.Load(restored, path);
            Assert.Equal(terrain, Assert.Single(restored.Objects).Id);
            SceneFile.Save(restored, path);
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            var saved = json.RootElement.GetProperty("LocalTerrain");
            Assert.Equal(terrain, saved.GetProperty("EntityId").GetGuid());
            Assert.Equal(5, saved.GetProperty("SamplesX").GetInt32());
            Assert.Equal(5, saved.GetProperty("SamplesY").GetInt32());
            Assert.Equal(25, saved.GetProperty("Heights").GetArrayLength());
            Assert.Equal(17.25f, saved.GetProperty("Heights")[17].GetSingle());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void OrphanTerrainFile_StillRejectedWithoutReplacingScene()
    {
        var scene = new TerrainPreviewSession(TerrainPreviewMap.CreateBlank(4, 4), centered: true).Scene;
        string path = TestPath();
        try
        {
            SceneFile.Save(scene, path);
            var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            json["LocalTerrain"]!["EntityId"] = Guid.NewGuid().ToString();
            File.WriteAllText(path, json.ToJsonString());
            var target = PreviewSeed.Create3D();
            var before = Assert.Single(target.World.Entities);
            Assert.Throws<InvalidDataException>(() => SceneFile.Load(target, path));
            Assert.Equal(before, Assert.Single(target.World.Entities));
        }
        finally { File.Delete(path); }
    }

    private static string TestPath() => Path.Combine(
        AppContext.BaseDirectory, $"E602BR01-{Guid.NewGuid():N}.xyscene");
}