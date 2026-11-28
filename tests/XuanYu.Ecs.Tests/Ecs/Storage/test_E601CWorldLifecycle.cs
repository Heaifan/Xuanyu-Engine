using Xunit;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601CWorldLifecycle
{
    [Fact]
    public void Create_UsesRegistryIdentityAndEmptyTable()
    {
        var world = new EcsWorld();
        Guid guid = Guid.NewGuid();
        EntityId id = world.Create(guid);
        Assert.True(world.IsAlive(id));
        Assert.True(world.TryGetId(guid, out var found));
        Assert.Equal(id, found);
        Assert.True(world.TryGetGuid(id, out var actual));
        Assert.Equal(guid, actual);
        Assert.Equal(1, world.Count);
        Assert.Equal(id, Assert.Single(Assert.Single(world.Query()).Entities.ToArray()));
        Assert.Throws<ArgumentException>(() => world.Create(guid));
        Assert.Throws<ArgumentException>(() => world.Create(Guid.Empty));
        Assert.Equal(1, world.Count);
    }

    [Fact]
    public void Destroy_RepairsMovedRowAndReusesIndexWithoutData()
    {
        var world = new EcsWorld();
        var first = world.Create();
        var second = world.Create();
        world.Add(first, 11);
        world.Add(second, 22);
        Assert.True(world.TryGetGuid(first, out var guid));
        Assert.True(world.Destroy(first));
        Assert.False(world.TryGetId(guid, out _));
        Assert.False(world.Destroy(first));
        Assert.Equal(22, world.Get<int>(second));
        var reused = world.Create();
        Assert.Equal(first.Index, reused.Index);
        Assert.NotEqual(first.Generation, reused.Generation);
        Assert.False(world.Has<int>(reused));
        Assert.False(world.IsAlive(first));
        Assert.Equal(2, world.Count);
        Assert.True(world.Destroy(second));
        Assert.Empty(world.Query(ComponentType.Of<int>()));
    }

    [Fact]
    public void InvalidHandles_CannotReadWriteOrChangeStructure()
    {
        var world = new EcsWorld();
        var stale = world.Create();
        world.Destroy(stale);
        var live = world.Create();
        world.Add(live, 9);
        Assert.Throws<KeyNotFoundException>(() => world.Get<int>(stale));
        Assert.Throws<KeyNotFoundException>(() => world.Set(stale, 8));
        Assert.Throws<KeyNotFoundException>(() => world.Has<int>(stale));
        Assert.Throws<KeyNotFoundException>(() => world.Add(stale, 8d));
        Assert.Throws<KeyNotFoundException>(() => world.Remove<int>(stale));
        Assert.Throws<KeyNotFoundException>(() => world.Get<int>(default));
        Assert.Equal(9, world.Get<int>(live));
        Assert.Equal(1, world.Count);
    }

    [Fact]
    public void DuplicateAddAndMissingRemove_DoNotChangeData()
    {
        var world = new EcsWorld();
        var id = world.Create();
        Assert.Throws<KeyNotFoundException>(() => world.Get<int>(id));
        Assert.Throws<KeyNotFoundException>(() => world.Set(id, 3));
        Assert.True(world.Add(id, 5));
        Assert.False(world.Add(id, 9));
        Assert.False(world.Remove<double>(id));
        Assert.Equal(5, world.Get<int>(id));
        world.Set(id, 7);
        Assert.Equal(7, world.Get<int>(id));
        Assert.Equal(1, world.Count);
    }
}