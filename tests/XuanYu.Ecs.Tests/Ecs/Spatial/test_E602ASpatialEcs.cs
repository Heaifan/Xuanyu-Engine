using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.Ecs.Tests.Ecs;

public sealed class test_E602ASpatialEcs
{
    [Fact]
    public void SpatialComponentSurvivesMigrationAndQuery()
    {
        var world = new EcsWorld();
        var entity = world.Create();
        var point = SpatialPosition.FromGeo(new GeoCoordinate(24.5, 121.5, 100));
        Assert.True(world.Add(entity, point));
        Assert.True(world.Add(entity, 42));
        Assert.Equal(point, world.Get<SpatialPosition>(entity));
        var view = Assert.Single(world.Query(), item => item.Has<SpatialPosition>() && item.Count > 0);
        Assert.Equal(entity, view.Entities[0]);
        Assert.Equal(point, view.GetColumn<SpatialPosition>()[0]);
        Assert.True(world.Remove<int>(entity));
        Assert.Equal(point, world.Get<SpatialPosition>(entity));
    }

    [Fact]
    public void WorldClonePreservesPositionAndIsolatesChanges()
    {
        var world = new EcsWorld();
        var guid = Guid.NewGuid();
        var entity = world.Create(guid);
        var point = SpatialPosition.FromGeo(new GeoCoordinate(24.5, 121.5, 100));
        Assert.True(world.Add(entity, point));
        var clone = world.Clone();
        Assert.True(clone.TryGetId(guid, out var copy));
        Assert.Equal(point, clone.Get<SpatialPosition>(copy));
        var moved = SpatialPosition.FromGeo(new GeoCoordinate(24.6, 121.5, 100));
        clone.Set(copy, moved);
        Assert.Equal(moved, clone.Get<SpatialPosition>(copy));
        Assert.Equal(point, world.Get<SpatialPosition>(entity));
    }
}