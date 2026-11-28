using System.Numerics;
using System.Text.Json;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests;

public sealed class test_E602BSpatialIntegration
{
    [Fact]
    public void SceneFileRoundTripsPreciseLocalPosition()
    {
        string path = Path.Combine(AppContext.BaseDirectory, $"E602B-{Guid.NewGuid():N}.xyscene");
        try
        {
            var scene = PreviewSeed.Create3D();
            var id = scene.Objects[0].Id;
            var point = SpatialPosition.FromLocal(1_000_000.01, 2.02, 3.03);
            scene.World.SetSpatialPosition(id, point);
            SceneFile.Save(scene, path);
            Assert.Contains("ExactPosition", File.ReadAllText(path));
            var restored = PreviewSeed.Create3D();
            SceneFile.Load(restored, path);
            Assert.Equal(point, restored.World.GetSpatialPosition(id));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void LegacyTransformJsonRetainsItsShapeAndMeaning()
    {
        var options = new JsonSerializerOptions { IncludeFields = true };
        const string json = """
            {"Position":{"X":2.5,"Y":-1,"Z":3},"Rotation":{"X":0,"Y":0,"Z":0,"W":1},
             "Scale":{"X":1,"Y":1,"Z":1}}
            """;
        var transform = JsonSerializer.Deserialize<Transform>(json, options);
        Assert.Equal(new Transform(new Vector3(2.5f, -1, 3), Quaternion.Identity, Vector3.One), transform);
        Assert.DoesNotContain("ExactPosition", JsonSerializer.Serialize(transform, options));
    }

    [Fact]
    public void LocalRenderBridgeDoesNotDrawGlobalEntity()
    {
        var world = new WorldScene();
        var local = world.Create("Local");
        var global = world.Create("Global", SpatialPosition.FromGeo(new GeoCoordinate(24.5, 121.5, 100)));
        var bridge = new WorldRenderBridge(world);
        var mesh = new RenderMesh("test/spatial", new float[21]);
        bridge.Bind(local.Id, mesh);
        bridge.Bind(global.Id, mesh);
        var draw = Assert.Single(bridge.Snapshot(Matrix4x4.Identity));
        Assert.Equal(Matrix4x4.Identity, draw.ModelToClip);
    }

    [Fact]
    public void SceneFileDoesNotSilentlyDiscardGlobalEntity()
    {
        string path = Path.Combine(AppContext.BaseDirectory, $"E602B-{Guid.NewGuid():N}.xyscene");
        try
        {
            var scene = PreviewSeed.Create3D();
            scene.World.Create("Global", SpatialPosition.FromGeo(new GeoCoordinate(24.5, 121.5, 100)));
            Assert.Throws<InvalidOperationException>(() => SceneFile.Save(scene, path));
            Assert.False(File.Exists(path));
        }
        finally { File.Delete(path); }
    }
}