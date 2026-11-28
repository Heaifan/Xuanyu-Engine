using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.World;
using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C02R01TransformHistory
{
    [Fact]
    public void DirectEdit_UndoRedoAndNewBranch()
    {
        var scene = new PreviewScene();
        var cube = scene.CreateCube();
        var initial = scene.World.Find(cube.Id)!.Transform;
        var next = initial with { Position = new Vector3(10, 20, 30) };
        scene.SetTransform(cube.Id, next);
        scene.UndoTransform();
        Assert.Equal(initial, scene.World.Find(cube.Id)!.Transform);
        Assert.Equal(cube.Id, scene.SelectedId);
        scene.RedoTransform();
        Assert.Equal(next, scene.World.Find(cube.Id)!.Transform);
        scene.UndoTransform();
        scene.SetTransform(cube.Id, initial with { Scale = new Vector3(2) });
        Assert.False(scene.CanRedoTransform);
    }

    [Fact]
    public void DragIsOneEntryAndCancelDoesNotInvalidateRedo()
    {
        var scene = new PreviewScene(); var cube = scene.CreateCube();
        var initial = scene.World.Find(cube.Id)!.Transform;
        scene.BeginTransformEdit(cube.Id);
        for (int i = 1; i <= 20; i++) scene.SetTransform(cube.Id, initial with { Position = new Vector3(i, 0, 0) });
        Assert.False(scene.CanUndoTransform);
        scene.EndTransformEdit(true);
        scene.UndoTransform();
        Assert.Equal(initial, scene.World.Find(cube.Id)!.Transform);
        Assert.False(scene.CanUndoTransform);
        scene.BeginTransformEdit(cube.Id);
        scene.SetTransform(cube.Id, initial with { Position = Vector3.One });
        scene.EndTransformEdit(false);
        Assert.Equal(initial, scene.World.Find(cube.Id)!.Transform);
        Assert.True(scene.CanRedoTransform);
        scene.RedoTransform();
        Assert.Equal(new Vector3(20, 0, 0), scene.World.Find(cube.Id)!.Transform.Position);
    }

    [Fact]
    public void DeleteAndReloadClearTransformHistory()
    {
        var scene = new PreviewScene(); var cube = scene.CreateCube();
        scene.SetTransform(cube.Id, Transform.Identity with { Position = Vector3.One });
        Assert.True(scene.CanUndoTransform);
        scene.Delete(cube.Id);
        Assert.False(scene.CanUndoTransform);
        cube = scene.CreateCube();
        var path = Path.Combine(AppContext.BaseDirectory, $"XYE-C02R01-{Guid.NewGuid():N}.xyscene");
        try
        {
            SceneFile.Save(scene, path);
            scene.SetTransform(cube.Id, Transform.Identity with { Position = Vector3.One });
            Assert.True(scene.CanUndoTransform);
            SceneFile.Load(scene, path);
            Assert.False(scene.CanUndoTransform);
            Assert.False(scene.CanRedoTransform);
            Assert.Equal(Transform.Identity, scene.World.Find(cube.Id)!.Transform);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void GeographicHistoryRetainsExactDoublePrecision()
    {
        var scene = new PreviewScene();
        var initial = SpatialPosition.FromEcef(Wgs84.ToEcef(new GeoCoordinate(23.5, 121.1, 123.456)));
        var cube = scene.CreateGeographicCube(initial);
        var before = scene.World.Find(cube.Id)!.Transform;
        var moved = SpatialPosition.FromEcef(Wgs84.ToEcef(new GeoCoordinate(23.5000001, 121.1, 123.457)));
        var after = Transform.FromSpatial(moved, before.Rotation, before.Scale);
        scene.SetTransform(cube.Id, after);
        scene.UndoTransform();
        Assert.Equal(before, scene.World.Find(cube.Id)!.Transform);
        scene.RedoTransform();
        Assert.Equal(after, scene.World.Find(cube.Id)!.Transform);
    }
}