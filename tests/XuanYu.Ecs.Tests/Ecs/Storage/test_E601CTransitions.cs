using Xunit;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601CTransitions
{
    private readonly record struct Position(int X, int Y);

    [Fact]
    public void Migration_PreservesIdentityAndSharedColumns()
    {
        var world = new EcsWorld();
        var guid = Guid.NewGuid();
        var id = world.Create(guid);
        var position = new Position(4, 7);
        world.Add(id, position);
        world.Add(id, 13);
        world.Add(id, 2.5d);
        Assert.True(world.Remove<int>(id));
        Assert.Equal(position, world.Get<Position>(id));
        Assert.Equal(2.5d, world.Get<double>(id));
        Assert.False(world.Has<int>(id));
        Assert.True(world.TryGetId(guid, out var found));
        Assert.Equal(id, found);
        Assert.True(world.TryGetGuid(id, out var actual));
        Assert.Equal(guid, actual);
        Assert.Equal(1, world.Count);
    }

    [Fact]
    public void Migration_RepairsBothSourceAndDestinationMappings()
    {
        var world = new EcsWorld();
        var first = world.Create();
        var second = world.Create();
        world.Add(first, 11);
        world.Add(second, 22);
        world.Add(first, 1.5d);
        Assert.Equal(22, world.Get<int>(second));
        world.Set(second, 23);
        world.Add(second, 2.5d);
        world.Remove<double>(first);
        Assert.Equal(23, world.Get<int>(second));
        Assert.Equal(2.5d, world.Get<double>(second));
        Assert.Equal(11, world.Get<int>(first));
        Assert.True(world.Destroy(second));
        Assert.Equal(11, world.Get<int>(first));
    }

    [Fact]
    public void SameComponentSet_UsesSameTableRegardlessOfOrder()
    {
        var world = new EcsWorld();
        var a = world.Create();
        var b = world.Create();
        world.Add(a, 1);
        world.Add(a, 2d);
        world.Add(b, 3d);
        world.Add(b, 4);
        var table = Assert.Single(world.Query(ComponentType.Of<int>(), ComponentType.Of<double>()));
        Assert.Equal(2, table.Count);
        Assert.Contains(a, table.Entities.ToArray());
        Assert.Contains(b, table.Entities.ToArray());
        Assert.Equal(4, world.Get<int>(b));
        world.Remove<double>(a);
        world.Add(a, 5d);
        Assert.Equal(2, Assert.Single(world.Query(ComponentType.Of<double>())).Count);
        Assert.Equal(5d, world.Get<double>(a));
    }

    [Fact]
    public void RemoveAll_ProducesEmptyEntityAndReAddUsesNewValue()
    {
        var world = new EcsWorld();
        var id = world.Create();
        world.Add(id, 15);
        world.Add(id, 8d);
        world.Remove<int>(id);
        world.Remove<double>(id);
        Assert.Empty(world.Query(ComponentType.Of<int>()));
        var empty = Assert.Single(world.Query());
        Assert.Equal(1, empty.Count);
        Assert.False(empty.Has<int>());
        Assert.True(world.Add<int>(id));
        Assert.Equal(0, world.Get<int>(id));
        Assert.Equal(1, world.Count);
    }
}