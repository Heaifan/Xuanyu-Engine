using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.Ecs.Tests.Ecs;

public sealed class test_E602BSceneSpatial
{
    [Fact]
    public void LegacyTransformUpdatesTheAuthoritativePosition()
    {
        var scene = new WorldScene();
        var id = scene.Create("Local").Id;
        Assert.Equal(default(SpatialPosition), scene.GetSpatialPosition(id));
        var transform = Transform.Identity with { Position = new Vector3(1, 2, 3) };
        scene.SetTransform(id, transform);
        Assert.Equal(SpatialPosition.FromLocal(1, 2, 3), scene.GetSpatialPosition(id));
        Assert.Equal(transform, scene.Find(id)!.Transform);
    }

    [Fact]
    public void RotationAndScalePreserveDoublePosition()
    {
        var scene = new WorldScene();
        var p = SpatialPosition.FromLocal(1_000_000.01, -2_000_000.02, 3_000_000.03);
        var entity = scene.Create("Precise", p);
        var transform = entity.Transform with
        {
            Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.4f),
            Scale = new Vector3(2, 3, 4)
        };
        scene.SetTransform(entity.Id, transform);
        Assert.Equal(p, scene.GetSpatialPosition(entity.Id));
        Assert.Equal(transform, scene.Find(entity.Id)!.Transform);
        Assert.Equal(transform, scene.GetComponent<Transform>(entity.Id));
    }

    [Fact]
    public void GlobalPositionDoesNotExposeAnUnreferencedLocalMatrix()
    {
        var scene = new WorldScene();
        var p = SpatialPosition.FromGeo(new GeoCoordinate(24.5, 121.5, 100));
        var entity = scene.Create("Global", p);
        var moved = SpatialPosition.FromGeo(new GeoCoordinate(24.6, 121.5, 100));
        scene.SetSpatialPosition(entity.Id, moved);
        Assert.Equal(moved, scene.GetSpatialPosition(entity.Id));
        var transform = scene.Find(entity.Id)!.Transform;
        Assert.Equal(moved, transform.Spatial);
        Assert.Throws<InvalidOperationException>(() => { _ = transform.Position; });
        Assert.Throws<InvalidOperationException>(() => transform.ToMatrix());
        Assert.Contains("Ecef", transform.ToString());
    }

    [Fact]
    public void GenericComponentApiCannotBypassRequiredPosition()
    {
        var scene = new WorldScene();
        var id = scene.Create("A").Id;
        var p = SpatialPosition.FromLocal(1_000_000.01, 2, 3);
        Assert.True(scene.HasComponent<SpatialPosition>(id));
        Assert.False(scene.AddComponent(id, p));
        Assert.Throws<InvalidOperationException>(() => scene.RemoveComponent<SpatialPosition>(id));
        scene.SetComponent(id, p);
        Assert.Equal(p, scene.GetComponent<SpatialPosition>(id));
        var transform = scene.GetComponent<Transform>(id) with { Scale = new Vector3(2) };
        scene.SetComponent(id, transform);
        Assert.Equal(p, scene.GetSpatialPosition(id));
        Assert.Equal(transform, scene.Find(id)!.Transform);
    }

    [Fact]
    public void CloneAndReplacePreservePreciseAndGlobalPositions()
    {
        var scene = new WorldScene();
        var a = scene.Create("A", SpatialPosition.FromLocal(1_000_000.01, 2, 3));
        var b = scene.Create("B", SpatialPosition.FromGeo(new GeoCoordinate(24.5, 121.5, 100)));
        var clone = scene.Clone();
        clone.SetSpatialPosition(a.Id, SpatialPosition.FromLocal(9, 8, 7));
        Assert.Equal(a.Transform.Spatial, scene.GetSpatialPosition(a.Id));
        clone.Replace(scene.Entities.ToArray());
        Assert.Equal(a, clone.Find(a.Id));
        Assert.Equal(b, clone.Find(b.Id));
    }

    [Fact]
    public void LocalAndGlobalSpacesCannotBeChangedImplicitly()
    {
        var scene = new WorldScene();
        var point = SpatialPosition.FromGeo(new GeoCoordinate(24.5, 121.5, 100));
        var entity = scene.Create("Global", point);
        Assert.Throws<InvalidOperationException>(() => scene.SetTransform(entity.Id, Transform.Identity));
        Assert.Throws<InvalidOperationException>(() => scene.SetComponent(entity.Id, default(SpatialPosition)));
        Assert.Equal(entity, scene.Find(entity.Id));
    }
}